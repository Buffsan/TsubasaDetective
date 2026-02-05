using TMPro;
using UnityEngine;


public class Boss_Controller : EnemyBase
{
    [SerializeField] Boss_MoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;

    [SerializeField] CutScene StartCutScene;
    
    [SerializeField] AudioClip StartAudio;
 
    public TextMeshProUGUI HPtext;
    float movieCount = 0;
    
    private void Start()
    {
        if (moveBase == null)
        {
            GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject, transform.position, Quaternion.identity);
            moveBase = CL_MoveBase.GetComponent<Boss_MoveBase>();
            CL_MoveBase.transform.SetParent(gameObject.transform);
        }
        


        moveBase.enemyBase = this;


        if (status == Status.MovieStay) return;
        ALL_System.systemManager.AllEnemy.Add(this.gameObject);

    }
    private void FixedUpdate()
    {
        switch (status)
        {
            case Status.Nomal:
                isNomalActive();
                break;
            case Status.Stan:
                isStanStan();
                break;
            case Status.ConfusionResistance:
                isConfusion();
                break;
            case Status.Die:
                foreach (var enemys in moveBase.AllEnemy) 
                {
                    if (enemys)Destroy(enemys.gameObject);
                }
                moveBase.enemyList.Clear();
                return; // éÄñSíÜÇÕàÍêÿìÆÇ©Ç≥Ç»Ç¢
            case Status.MovieStay:
                isMovie();
                break;
        }
        HP_TextChange();
        
    }
    void HP_TextChange() 
    {

        HPtext.text = MAXHP + "/" + HP; 

    }
    // Update is called once per frame
    void isMovie() 
    {
        if (MoviePhase == 0) 
        {
            audioManager.BgmSource.Stop();
            movieCount += Time.deltaTime;
            if (movieCount < 0.2) return;
            if (ALL_System.systemManager.gameMode != SystemManager.GameMode.Nomal) return;
            ALL_System.systemManager.gameMode = SystemManager.GameMode.Movie;
            
            movieCount =0;
            MoviePhase ++;
        }
        if (MoviePhase == 1) 
        { 
            charaDamage.gameObject.SetActive(false);
            float PlayerDistanceNow = Vector2.Distance(playerController.transform.position, transform.position);
            if (PlayerDistanceNow < 3) 
            { 
                MoviePhase ++;
                playerController.movetype = PlayerController.MoveType.Wait;
                playerController.rb.velocity = Vector2.zero;
                playerController.MoveInput = Vector2.zero;

                audioManager.PlaySE(StartAudio);
            }
        }
        if (MoviePhase == 2) 
        {
            StartCutScene.animator.Play("èoåª");
            StartCutScene.ChangeImage(CutSceneData);
            MoviePhase++;
        }
        if (MoviePhase == 3) 
        {
            movieCount += Time.deltaTime;
            if (movieCount < 1) return;
            if (playerController.MoveInput != Vector2.zero) 
            {
                ALL_System.systemManager.gameMode = SystemManager.GameMode.Nomal;
                playerController.movetype = PlayerController.MoveType.Nomal;
                MoviePhase++;
                audioManager.BgmSource.Play();
                StartCutScene.animator.Play("è¡Ç¶ÇÈ");
                charaDamage.gameObject.SetActive(true);
                ALL_System.systemManager.AllEnemy.Add(this.gameObject);
                status = Status.Nomal;
            }
        }
        if (MoviePhase == 4) 
        { 
        
        }
    
    }

    void isConfusion()
    {
        animator.SetBool("Damage", true);
        if (ConfusionTimer + 3 > StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {
            moveBase.M_Con();
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    void isStanStan()
    {
        animator.SetBool("Damage", true);
        if (StanTime > StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {
            moveBase.M_Stan();
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    private void isNomalActive()
    {
        switch (moveType)
        {
            case MoveType.Move:
                isMove();
                break;
            case MoveType.Attack:
                isAttack();
                break;
                case MoveType.AI:
                //isAI();
                break;

        }

        
    }
    void isAI() 
    {
        moveBase.M_AI();
    }
    void isAttack()
    {//çUåÇÉÇÅ[ÉVÉáÉìÇï ìrì¸ÇÍÇÈ
        moveBase.M_Attack();
    }
    private void isMove()
    {
        //moveBase.M_BaseMove();
        isAI();
    }
    public void isTargetLook(Vector2 target) 
    {
        if (target.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-DefoScale, DefoScale);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(DefoScale, DefoScale);
        }
    }
    public void isPlayerLook()
    {
        PlayerDirection = Player.transform.position - transform.position;
        PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
        if (Player.transform.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-DefoScale, DefoScale);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(DefoScale, DefoScale);
        }
    }
}
