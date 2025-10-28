using UnityEngine;
using UnityEngine.Pool;


public class Bullet : MonoBehaviour
{

    private BulletType type;
    private Vector3 bulletSpawnLoc;

    // Reference to the object pool
    private ObjectPool<Bullet> pool;

    // Method to set the pool reference
    public void SetPool(ObjectPool<Bullet> bulletPool)
    {
        pool = bulletPool;
    }


    public void Initialize(BulletType bulletType, Vector3 spawnLocation)
    {
        type = bulletType;
        transform.position = spawnLocation;
        bulletSpawnLoc = spawnLocation;
    }


    private void Update()
    {
        // Move the bullet forward
        transform.Translate(type.stats.speed * Time.deltaTime * Vector3.forward);

        // Check if the bullet has exceeded its range then return it to the pool
        if (Vector3.Distance(bulletSpawnLoc,transform.position) >= type.stats.range)
        {
            pool.Release(this);
        }
    }




    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a IDamagable component and apply damage

        if (other.TryGetComponent<IDamagable>(out IDamagable damageable))
        {
            damageable.TakeDamage(type.stats.damage);
        }
        
        // Return bullet to pool when it hits something
        pool.Release(this);
    }

}
