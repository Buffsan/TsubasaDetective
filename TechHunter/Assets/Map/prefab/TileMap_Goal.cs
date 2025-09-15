using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap_Goal : MonoBehaviour
{

    SystemManager system => SystemManager.Instance;
    [SerializeField] ParticleSystem WallEffect;
    [SerializeField] GameObject Wall;

    public enum ModeType 
    { 
    
        Start,
        Goal
    
    }
    public ModeType Mode = ModeType.Goal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mode == ModeType.Goal)
        {
            if (system.gameMode == SystemManager.GameMode.Goal)
            {

                WallEffect.emissionRate = 0;
                Wall.SetActive(false);

            }
        } else if(Mode == ModeType.Start) 
        {
            if (system.gameMode == SystemManager.GameMode.Nomal)
            {

  
                Wall.SetActive(true);

            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && system.gameMode == SystemManager.GameMode.Goal&&Mode == ModeType.Goal) 
        {
            system.isGameStart();
        }  
    }
}
