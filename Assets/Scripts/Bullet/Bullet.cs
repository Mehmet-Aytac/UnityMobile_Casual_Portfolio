using UnityEngine;
using UnityEngine.Pool;


public class Bullet : MonoBehaviour
{

    private BulletType type;
    private Vector3 bulletSpawnLoc;
    private Vector3 velocity;

    // Reference to the object pool
    private ObjectPool<Bullet> pool;

    // Method to set the pool reference
    public void SetPool(ObjectPool<Bullet> bulletPool)
    {
        pool = bulletPool;
    }

    public void ResetState()
    {
        type = null;
    }


    public void Initialize(BulletType bulletType, Vector3 spawnLocation)
    {
        type = bulletType;
        transform.position = spawnLocation;
        bulletSpawnLoc = spawnLocation;
        velocity = Vector3.forward * type.stats.speed;
    }


    void Update()
    {
        // Move the bullet forward
        transform.position += velocity * Time.deltaTime;

        // Check if the bullet has exceeded its range then return it to the pool
        if ((transform.position - bulletSpawnLoc).sqrMagnitude >= type.stats.range * type.stats.range)
            pool.Release(this);
    }
  

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a IDamagable component and apply damage

        if (other.TryGetComponent<IDamagable>(out IDamagable damageable))
        {
            damageable.TakeDamage(type.stats.damage);
        }
        if (pool == null) return;
        // Return bullet to pool when it hits something
        pool.Release(this);
    }

}