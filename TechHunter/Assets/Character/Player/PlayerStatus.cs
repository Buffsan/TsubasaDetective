using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public float Add_AdditiveATK = 0;
    public float AddATK = 0;
    public float AddAtkDF = 0;
    public float AddRegen = 0;
    public float AddRecoveryHP = 0;
    public float AddHP = 0;
    public float AddRange = 0;
    public float AddCritical = 0;
    public float AddSpeed = 0;
    public float AddConfusionTime = 0;
    // Additional status-card percentages. These stack additively.
    public float AddCoinGainPercent = 0f;
    public float AddConfusionDamagePercent = 0f;
    public float AddHealingPercent = 0f;
    public float AddLuckyDamagePercent = 0f;

    // Preserve fractional coin rewards, including pooled coin denominations.
    float coinGainRemainder = 0f;
    public int CalculateCollectedCoins(int baseCoins)
    {
        if (baseCoins <= 0) return 0;
        float total = baseCoins * Mathf.Max(0f, 1f + AddCoinGainPercent / 100f)
            + coinGainRemainder;
        int received = Mathf.FloorToInt(total + 0.00001f);
        coinGainRemainder = Mathf.Max(0f, total - received);
        return received;
    }

    public float AddMultiplierATK = 1;

    public float Critical = 0;
    public float Range = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
