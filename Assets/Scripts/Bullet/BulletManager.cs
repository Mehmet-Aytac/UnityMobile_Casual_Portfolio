using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour, IBulletHitHandler
{
    public struct RuntimeBulletStats
    {
        public float speed;
        public float damage;
        public float range;
        public BulletType bulletType;
    }

    public BulletType defaultBulletType;
    public RuntimeBulletStats defaultStats;
    public RuntimeBulletStats currentStats;

    private readonly List<Bullet> activeBullets = new();

    public void Awake()
    {
        defaultStats = new RuntimeBulletStats
        {
            speed = 50f,
            damage = 5f,
            range = 100f,
            bulletType = defaultBulletType
        };
    }

    public void Start()
    {
        if (currentStats.bulletType == null)
            currentStats = defaultStats;
    }


    public void RegisterBullet(Bullet bullet)
    {
        if (!activeBullets.Contains(bullet))
            activeBullets.Add(bullet);
    }

    public void UnregisterBullet(Bullet bullet)
    {
        activeBullets.Remove(bullet);
    }

    void Update()
    {
        // Example: manager controls movement and range
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            var b = activeBullets[i];
            b.transform.position += Vector3.forward * currentStats.speed * Time.deltaTime;

            if ((b.transform.position - b.SpawnPos).sqrMagnitude >= currentStats.range * currentStats.range)
            {
                b.Return();
                activeBullets.RemoveAt(i);
            }
        }
    }

    public void OnBulletHit(Bullet bullet, Collider other)
    {
        if (other.TryGetComponent<IDamagable>(out var dmg))
            dmg.TakeDamage(currentStats.damage);

        bullet.Return();
        UnregisterBullet(bullet);
    }

    
    public void SetBulletTypeİ(BulletType type)
    {
        currentStats.bulletType = type;
    }

}