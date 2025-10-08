using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public Vector3 MoveCamera = new Vector3 (5, 5,-10);
    [SerializeField] GameObject Player;

    Vector3 NowPos;
    public Vector3 AddRange = Vector2.zero;

    private Coroutine shakeCoroutine; // 現在再生中のコルーチンを保持
    private Vector3 initialPosition;  // 元の位置を保持

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        NowPos = new Vector3(Player.transform.position.x, Player.transform.position.y, MoveCamera.z);
        if (NowPos.x > MoveCamera.x)
        {
            NowPos = new Vector3(MoveCamera.x, NowPos.y, -10);
        }
        else if(NowPos.x < -MoveCamera.x)
        {
            NowPos = new Vector3(-MoveCamera.x, NowPos.y, -10);
        }
        if (NowPos.y > MoveCamera.y)
        {
            NowPos = new Vector3(NowPos.x,MoveCamera.y, -10);
        }
        else if (NowPos.y < -MoveCamera.y)
        {
            NowPos = new Vector3(NowPos.x, -MoveCamera.y,-10);
        }
        transform.position = new Vector3(NowPos.x + AddRange.x, NowPos.y + AddRange.y, NowPos.z);
    }

    public void Shake(float duration, float strength)
    {
        // 既に揺れ中なら停止してリセット
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            NowPos = Vector3.zero;
        }

        // 新しい揺れ開始
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // ランダムにカメラ位置をずらす
            AddRange = (Vector3)Random.insideUnitCircle * strength;

 

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 元の位置に戻す
        AddRange = Vector2.zero;
        shakeCoroutine = null;
    }

    public void isChangeNumber(Vector2 Newpos)
    {
        MoveCamera = Newpos;
    }
}
