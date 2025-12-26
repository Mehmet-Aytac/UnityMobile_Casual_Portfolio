using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private IBulletHitHandler hitHandler;
    private ObjectPool<Bullet> pool;

    public Vector3 SpawnPos { get; private set; }

    public void Initialize(Vector3 spawnPos, IBulletHitHandler handler)
    {
        SpawnPos = spawnPos;
        hitHandler = handler;
        transform.position = spawnPos;
    }

    public void SetPool(ObjectPool<Bullet> bulletPool)
    {
        pool = bulletPool;
    }

    public void Return()
    {
        if (pool != null)
            pool.Release(this);
    }

    void OnTriggerEnter(Collider other)
    {
        // Just forward the event; manager decides what to do
        hitHandler?.OnBulletHit(this, other);
    }
}
