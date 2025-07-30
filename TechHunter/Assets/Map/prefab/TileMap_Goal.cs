using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap_Goal : MonoBehaviour
{

    SystemManager system => SystemManager.Instance;
    [SerializeField] ParticleSystem WallEffect;
    [SerializeField] GameObject Wall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (system.gameMode == SystemManager.GameMode.Goal) 
        {

            WallEffect.emissionRate = 0;
            Wall.SetActive(false);
        
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && system.gameMode == SystemManager.GameMode.Goal) 
        {
            system.isGameStart();
        }  
    }
}
