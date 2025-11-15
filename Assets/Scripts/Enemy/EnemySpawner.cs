using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    EnemyPoolManager poolManager;
    public EnemyType[] enemyTypes;

  
    void Start()
    {
        poolManager = ServiceLocator.Get<EnemyPoolManager>();

        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("EnemySpawner: enemyTypes array is empty!");
            return;
        }


        for(int i = 0; i < 10; i++)
        {
            // Example: spawn random type
            var randomType = enemyTypes[Random.Range(0, enemyTypes.Length)];
            poolManager.SpawnEnemy(randomType, new Vector3(0f, 3.1f, 200f));
        }
        
    }

    
}
