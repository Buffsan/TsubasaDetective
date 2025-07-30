using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnemySpawnData")]
public class EnemySpawnData : ScriptableObject
{
   
        public EnemySpawnBase enemyspawn;
  
        public List<EnemySpawnClass> enemyspawns = new List<EnemySpawnClass>();

        public int Level = 0;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SpawnEnemy()
        {
            foreach (EnemySpawnClass enemy in enemyspawns)
            {
                enemyspawn.isCloneEnemy(enemy.id, enemy.SpawnNumber);
            }

        }

    
}
