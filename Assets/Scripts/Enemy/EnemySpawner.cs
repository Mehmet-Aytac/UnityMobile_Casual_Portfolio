using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPoolManager poolManager;
    public EnemyType[] enemyTypes;

    void Start()
    {
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
