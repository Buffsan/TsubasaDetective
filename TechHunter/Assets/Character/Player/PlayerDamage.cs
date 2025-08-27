using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerController playerController;
    [SerializeField] AudioClip DamageClip;

    [SerializeField] GameObject DamageImage;
    Animator animator;

    [SerializeField] SpriteRenderer spriteRenderer;

    ALL_SystemManager systemManager => ALL_SystemManager.Instance;

    public enum Mode
    {

        Nomal,
        Invincible

    }
    public Mode mode = Mode.Nomal;

    float InvincibleCount = 0;
    public float InvincibleTime = 0;

    AudioManager audiomanager => AudioManager.instance;

    public float HP;
    public float DF;
    public float Stan;
    public float Regen;
    public float AddRegen;

    float StanCount = 0;
    float DieCount = 0;

    Vector2 SavePos;

    // Start is called before the first frame update
    void Start()
    {
        HP = playerController.charadata.MAXHP;
        DF = playerController.charadata.ATKDF;
        Stan = playerController.charadata.StanHP;
        Regen = playerController.charadata.Regen;

        animator = DamageImage.GetComponent<Animator>();    
    }
    private void FixedUpdate() 
    {
        if (mode == Mode.Invincible)
        {
            InvincibleCount += Time.deltaTime;
            if (InvincibleTime < InvincibleCount) 
            { 
            mode = Mode.Nomal;
                InvincibleCount = 0;
            }
        }
    }
    public void Damage(float Attackvalue, float Stanvalue, float value2, bool value3)
    {
        if (mode != Mode.Invincible && systemManager.systemManager.gameMode == SystemManager.GameMode.Nomal)
        {
            audiomanager.isPlaySE(DamageClip);
            float AddDamage = Attackvalue - (playerController.AddAtkDF + playerController.playerBuff.DEF_AllBuff) * playerController.playerBuff.MultiplyDEF_AllBuff;
            if (AddDamage > 0)
            {
                playerController.HP -= AddDamage;
                SetInvincible(0.5f);

                animator.Play("É_ÉÅÅ[ÉW");
            }

            HP_SliderChange();

            if (HP <= 0)
            {
                Death();

            }
        }
        

    }
    public void HP_SliderChange() 
    {
        playerController.HP_Slider.value = playerController.HP / playerController.MaxHP;
    }
    public void SetInvincible(float Time) 
    { 
    
        mode = Mode.Invincible;
        InvincibleCount = 0;
        InvincibleTime = Time;

    }

    public void Death() 
    { 
    Debug.Log("éÄÇÒÇæ");
    }
}
