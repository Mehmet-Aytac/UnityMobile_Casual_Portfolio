using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    private EnemyType type;

    // Reference to the object pool
    private ObjectPool<Enemy> pool;

    // Method to set the pool reference
    public void SetPool(ObjectPool<Enemy> enemyPool)
    {
        pool = enemyPool;
    }

    // Release from pool when enemy "dies"
    public void Die()
    {
        pool.Release(this);
    }



    public void Initialize(EnemyType enemyType, Vector3 spawnLocation)
    {
        type = enemyType;
        transform.position = spawnLocation;
    }


    private void Update()
    {
        // Move the enemy forward
        transform.Translate(type.stats.speed * Time.deltaTime * Vector3.forward);

        // Check if the enemy has exceeded its range(preset z value in this case) then return it to the pool
        if (transform.position.z <= -40)
        {
            pool.Release(this);
        }
    }

   
}