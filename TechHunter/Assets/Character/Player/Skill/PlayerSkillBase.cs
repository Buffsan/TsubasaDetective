using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillBase : MonoBehaviour
{
    public GameObject SkillWeapon;
    public GameObject SlillBullet;
    public PlayerController playerController => PlayerController.Instance;

    public Rigidbody2D Targetrb;

    public GameObject TargetObject;
    private void Awake()
    {
        TargetObject = playerController.gameObject;
    }

    public int SkillLevel =0;

    public enum ATT 
    { 
    A1, A2, A3, A4, A5, A6, A7,A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, A20, A21, A22, A23,
    }
    public ATT Att = ATT.A1;

    public float skillcount = 0;

    
    public GameObject SkillSpawn(GameObject Skill ,GameObject taget) 
    {
        GameObject CL_Weapon = Instantiate(Skill, taget.transform.position, Quaternion.identity);
        CL_Weapon.transform.parent = taget.transform;

        return CL_Weapon;
    }
    public GameObject SkillSpawn_Add(GameObject Skill, GameObject Spawntaget,GameObject Parenttaget)
    {
        GameObject CL_Weapon = Instantiate(Skill, Parenttaget.transform.position, Quaternion.identity);
        CL_Weapon.transform.parent = Spawntaget.transform;

        return CL_Weapon;
    }
    public void PlayerMove(float speed) 
    {
        if (TargetObject != playerController.gameObject) return;
        playerController.isMove(speed);
        playerController.NowSkillMove = speed;
    }
    public void PlayerVelocityMove(float speed,Vector2 Direction) 
    {
        Vector2 direction = Direction.normalized;
        if (TargetObject == playerController.gameObject)
        {
            playerController.rb.velocity = direction * speed;
        }
        else 
        {
            if (!Targetrb) Targetrb = TargetObject.GetComponent<Rigidbody2D>();
            if (!Targetrb) return;

            Targetrb.velocity = direction * speed;
        }
    }
    public virtual void SkillPlay()
    {

    }
    public void end()
    {
        if (TargetObject == playerController.gameObject)
        {
            playerController.mode = PlayerController.ModeType.M1;
            playerController.movetype = PlayerController.MoveType.Nomal;
        }

        Destroy(this.gameObject);
    }
}
