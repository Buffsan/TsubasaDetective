using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillBase : MonoBehaviour
{
    public GameObject SkillWeapon;
    public GameObject SlillBullet;
    public PlayerController playerController => PlayerController.Instance;

    public int SkillLevel =0;

    public enum ATT 
    { 
    A1, A2, A3, A4, A5, A6, A7,A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, A20, A21, A22, A23,
    }
    public ATT Att = ATT.A1;

    public float skillcount = 0;

    public virtual void SkillPlay()
    {

    }
    public void end()
    {
        playerController.mode = PlayerController.ModeType.M1;
        playerController.movetype = PlayerController.MoveType.Nomal;

        Destroy(this.gameObject);
    }
}
