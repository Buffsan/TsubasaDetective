using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObject_Character : MonoBehaviour
{
    [SerializeField] Animator AAAAAAAAAanimator;
    [SerializeField] string AnimationClipName;

    bool One = false;

    PlayerController playerController => PlayerController.Instance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!One)
        {
            if (other.CompareTag("PlayerAttack") )
            {

                if (playerController.transform.position.x > transform.position.x)
                {
                    transform.localScale = new Vector2(-1, 1);
                }
                else
                {
                    transform.localScale = new Vector2(-1, 1);
                }
                AAAAAAAAAanimator.Play(AnimationClipName);
                One = true;

            }
        }
    }
}
