using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_MolotovCocktail : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    public Vector2 endPos = Vector2.zero;
    public float duration = 1;
    public float arcHeight = 30;
    [SerializeField] GameObject Attack;

    private void Start()
    {
        StartCoroutine((MoveInArc()));
    }
    private IEnumerator MoveInArc()
    {
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = transform.position;
       
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;
        // BodyTypeを元に戻す
        rb.bodyType = originalBodyType;

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;

        GameObject CL_Attack = Instantiate(Attack,transform.position,Quaternion.identity);
    Destroy( CL_Attack ,2.6f);
        Destroy(gameObject);
    }
}
