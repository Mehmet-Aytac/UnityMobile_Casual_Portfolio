using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    // will create a CachedStats that holds all current weapon and bullet stats in memory. 
    // it will be used when spawning bullets.
    // it will only updated when something changes.
    // that way we don't have to calculate updated stats every Shoot() called.

    public struct CachedStats
    {
        public float bulletSpeed;
        public float bulletDamage;
        public float bulletRange;
        public float fireRate;
        public BulletType bulletType;
    }

    public BulletPoolManager bulletPool;
    public WeaponType weaponType;
    public Transform firePoint;

    private float fireTimer;
    private List<WeaponUpgrade> activeUpgrades = new();
    private CachedStats cachedStats;






    void RecalculateCachedStats()
    {
        cachedStats.bulletSpeed = CalculateBulletSpeed();
        cachedStats.bulletDamage = CalculateBulletDamage();
        cachedStats.bulletRange = CalculateBulletRange();
        cachedStats.fireRate = CalculateFireRate();
        cachedStats.bulletType = GetCurrentBulletType();

    }


    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= cachedStats.fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        BulletType type = cachedStats.bulletType;
        bulletPool.SpawnBullet(type, firePoint.position);
    }

    
   
    

    public void AddUpgrade(WeaponUpgrade upgrade)
    {
        if (upgrade.type == UpgradeType.ChangeBulletType)
        {
            foreach (var up in activeUpgrades)
                if (up.type == UpgradeType.ChangeBulletType)
                    activeUpgrades.Remove(up);
        }
        activeUpgrades.Add(upgrade);
        RecalculateCachedStats();
    }


    float CalculateBulletSpeed()
    {
        float speed = weaponType.bulletType.stats.speed;
        float totalUp = 0f;
        foreach (var up in activeUpgrades)
            if (up.type == UpgradeType.BulletSpeedPercent)
                totalUp *= up.value;
        return speed * totalUp;
    }

    float CalculateBulletDamage() 
    {
        float damage = weaponType.bulletType.stats.damage;
        float totalUp = 0f;
        foreach (var up in activeUpgrades)
            if (up.type == UpgradeType.DamagePercent)
                totalUp *= up.value;
        return damage * totalUp;
    }
    float CalculateBulletRange()
    {
        float range = weaponType.bulletType.stats.range;
        float totalUp = 0f;
        foreach (var up in activeUpgrades)
            if (up.type == UpgradeType.RangePercent)
                totalUp *= up.value;
        return range * totalUp;
    }


    float CalculateFireRate()
    {
        float rate = weaponType.fireRate;
        float totalUp = 0f;
        foreach (var up in activeUpgrades)
            if (up.type == UpgradeType.FireRatePercent)
                totalUp *= up.value;
        return rate * totalUp;
    }




    // This BulletType Upgrades Probably will not work.
    // Will look into how to change it with value in upgrade as index of BulletType.
    BulletType GetCurrentBulletType()
    {
        foreach (var up in activeUpgrades)
            if (up.type == UpgradeType.ChangeBulletType)
                return weaponType.bulletType;
        return weaponType.bulletType;
    }



    public BulletStats GetModifiedStats()
    {
        BulletStats stats = weaponType.bulletType.stats;
        foreach (var up in activeUpgrades)
        {
            switch (up.type)
            {
                case UpgradeType.DamagePercent:
                    stats.damage = Mathf.RoundToInt(stats.damage * (1 + up.value));
                    break;
                case UpgradeType.BulletSpeedPercent:
                    stats.speed *= 1 + up.value;
                    break;
                case UpgradeType.RangePercent:
                    stats.range *= 1 + up.value;
                    break;
            }
        }
        return stats;
    }
}