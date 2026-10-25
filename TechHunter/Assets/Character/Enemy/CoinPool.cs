using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coins keep their normal appearance while the scene is below the aggregation threshold.
/// Drop requests are evaluated together in LateUpdate before any new coins are activated.
/// </summary>
public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;

    [SerializeField] GameObject coinPrefab;
    [SerializeField, Min(1)] int initialCount = 50;
    [SerializeField, Min(1)] int aggregationThreshold = 150;
    [SerializeField, Min(1)] int maximumActiveCoins = 200;
    [SerializeField, Min(0.1f)] float mergeRadius = 1.5f;
    [SerializeField, Range(1, 24)] int maxCoinsPerCluster = 12;

    readonly Queue<GameObject> pool = new Queue<GameObject>();
    readonly HashSet<GameObject> activeCoins = new HashSet<GameObject>();
    readonly List<DropRequest> pending = new List<DropRequest>();
    readonly List<DropGroup> groups = new List<DropGroup>();
    CoinManager coinTemplate;
    long pendingNominalCount;

    readonly CoinManager.CoinType[] types =
    {
        CoinManager.CoinType.CrystalCoin,
        CoinManager.CoinType.GoaldCoin,
        CoinManager.CoinType.SilverCoin,
        CoinManager.CoinType.CopperCoin
    };
    readonly int[] values = new int[4];

    struct DropRequest
    {
        public Vector3 position;
        public int amount;
        public int nominalCount;
    }

    sealed class DropGroup
    {
        public Vector3 center;
        public long amount;
        public long nominalCount;
        public int requestCount;
        public readonly List<int> indices = new List<int>();

        public void Add(int index, DropRequest request)
        {
            center = (center * requestCount + request.position) / (requestCount + 1);
            requestCount++;
            amount += request.amount;
            nominalCount += request.nominalCount;
            indices.Add(index);
        }
    }

    public int ActiveCount => activeCoins.Count;
    public int ReservedRequestCount => pending.Count;

    void Awake()
    {
        Instance = this;
        coinTemplate = coinPrefab != null ? coinPrefab.GetComponent<CoinManager>() : null;
        if (coinTemplate == null)
        {
            Debug.LogError("CoinPool: coinPrefab must have a CoinManager.", this);
            enabled = false;
            return;
        }

        values[0] = coinTemplate.CrystalValue;
        values[1] = coinTemplate.GoaldValue;
        values[2] = coinTemplate.SilverValue;
        values[3] = coinTemplate.CopperValue;

        for (int i = 0; i < initialCount; i++)
            pool.Enqueue(CreateInactiveCoin());
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    GameObject CreateInactiveCoin()
    {
        GameObject obj = Instantiate(coinPrefab, transform);
        obj.SetActive(false);
        return obj;
    }

    // Enqueue only a value and a position. No coin is instantiated on the death callback.
    public void RequestDrop(int gold, Vector3 position)
    {
        if (gold <= 0 || coinTemplate == null) return;
        int expectedCount = CountNormalCoins(gold);
        pending.Add(new DropRequest
        {
            position = position,
            amount = gold,
            nominalCount = expectedCount
        });
        pendingNominalCount += expectedCount;
    }

    int CountNormalCoins(int gold)
    {
        long count = 0;
        int remaining = gold;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] <= 0) continue;
            count += remaining / values[i];
            remaining %= values[i];
        }
        if (remaining > 0) count++;
        return (int)System.Math.Min(count, int.MaxValue);
    }

    void LateUpdate()
    {
        if (pending.Count == 0) return;

        long projected = activeCoins.Count + pendingNominalCount;
        if (projected <= aggregationThreshold && projected <= maximumActiveCoins)
        {
            // Under the limit: preserve the original denominations, positions and counts.
            foreach (DropRequest request in pending)
                SpawnNormal(request.amount, request.position);
            pending.Clear();
            pendingNominalCount = 0;
            return;
        }

        // Over the limit: merge requests BEFORE creating physical objects.
        SpawnAggregated();
    }

    void SpawnNormal(int gold, Vector3 position)
    {
        int remaining = gold;
        for (int i = 0; i < values.Length; i++)
        {
            int denomination = values[i];
            if (denomination <= 0) continue;
            int count = remaining / denomination;
            remaining %= denomination;
            for (int j = 0; j < count; j++)
                SpawnCoin(types[i], denomination, position);
        }
        if (remaining > 0)
            SpawnCoin(CoinManager.CoinType.CopperCoin, remaining, position);
    }

    void SpawnAggregated()
    {
        int available = maximumActiveCoins - activeCoins.Count;
        if (available <= 0) return; // Keep the requests; pickup will free slots.

        groups.Clear();
        float radiusSqr = mergeRadius * mergeRadius;
        for (int i = 0; i < pending.Count; i++)
        {
            DropRequest request = pending[i];
            DropGroup nearest = null;
            float bestDistance = radiusSqr;
            foreach (DropGroup group in groups)
            {
                float distance = (group.center - request.position).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    nearest = group;
                }
            }

            if (nearest == null)
            {
                nearest = new DropGroup();
                groups.Add(nearest);
            }
            nearest.Add(i, request);
        }

        bool[] consumed = new bool[pending.Count];
        for (int i = 0; i < groups.Count && available > 0; i++)
        {
            DropGroup group = groups[i];
            // Reserve at most one coin for each nearby group that follows this one.
            int remainingGroups = groups.Count - i - 1;
            int reservedForOthers = Mathf.Min(remainingGroups, available - 1);
            int budget = Mathf.Min(maxCoinsPerCluster, available - reservedForOthers);
            budget = (int)System.Math.Min(budget, group.nominalCount);
            if (budget <= 0) continue;

            SpawnStacked(group.amount, group.center, budget);
            available -= budget;
            foreach (int index in group.indices)
                consumed[index] = true;
        }

        // A full pool can defer other locations without losing any reward value.
        for (int i = pending.Count - 1; i >= 0; i--)
            if (consumed[i]) pending.RemoveAt(i);

        pendingNominalCount = 0;
        foreach (DropRequest request in pending)
            pendingNominalCount += request.nominalCount;
    }

    void SpawnStacked(long gold, Vector3 position, int budget)
    {
        long[] count = new long[4];
        int[] slots = new int[4];
        long remaining = gold;
        long normalCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (values[i] <= 0) continue;
            count[i] = remaining / values[i];
            remaining %= values[i];
            normalCount += count[i];
        }
        if (remaining > 0)
        {
            // Any unusual coin denominations still preserve the exact remaining value.
            count[3]++;
            normalCount++;
        }

        int desired = (int)System.Math.Min(normalCount, budget);
        int used = 0;
        for (int i = 0; i < 4 && used < desired; i++)
        {
            if (count[i] <= 0) continue;
            slots[i] = 1;
            used++;
        }

        // Spread the remaining visual slots among denominations with the most coins.
        while (used < desired)
        {
            int best = -1;
            double bestScore = -1;
            for (int i = 0; i < 4; i++)
            {
                if (slots[i] == 0 || count[i] <= slots[i]) continue;
                double score = (double)count[i] / slots[i];
                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            if (best < 0) break;
            slots[best]++;
            used++;
        }

        // Denominations without their own physical coin are folded into the first coin.
        long carriedValue = remaining;
        for (int i = 0; i < 4; i++)
            if (slots[i] == 0) carriedValue += count[i] * values[i];

        bool appliedCarry = false;
        for (int i = 0; i < 4; i++)
        {
            if (slots[i] <= 0) continue;
            long perSlot = count[i] / slots[i];
            long remainder = count[i] % slots[i];
            for (int j = 0; j < slots[i]; j++)
            {
                long coinValue = (perSlot + (j < remainder ? 1 : 0)) * values[i];
                if (!appliedCarry)
                {
                    coinValue += carriedValue;
                    appliedCarry = true;
                }
                SpawnCoin(types[i], (int)coinValue, position);
            }
        }
    }

    // Keep this method for existing callers that request one individual coin.
    public GameObject GetCoin(Vector3 pos)
    {
        CoinManager coin = SpawnCoin(CoinManager.CoinType.CopperCoin,
            values[3] > 0 ? values[3] : 1, pos);
        return coin != null ? coin.gameObject : null;
    }

    CoinManager SpawnCoin(CoinManager.CoinType type, int amount, Vector3 pos)
    {
        if (activeCoins.Count >= maximumActiveCoins) return null;

        GameObject obj = pool.Count > 0 ? pool.Dequeue() : CreateInactiveCoin();
        obj.transform.position = pos;
        CoinManager coin = obj.GetComponent<CoinManager>();
        coin.InitializeCoin(type, amount);

        activeCoins.Add(obj);
        obj.SetActive(true);
        coin.Launch();

        SystemManager system = SystemManager.Instance;
        if (system != null)
        {
            system.coinManagers.Add(coin);
            if (system.gameMode == SystemManager.GameMode.Result)
                coin.moveType = CoinManager.MoveType.Chase;
        }
        return coin;
    }

    public void Release(GameObject obj)
    {
        if (!activeCoins.Remove(obj)) return;
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pool.Enqueue(obj);
    }
}
