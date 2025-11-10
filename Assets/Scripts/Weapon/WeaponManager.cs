using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// Manages the player's weapons, including firing logic and applying upgrades.
/// It keeps track of active weapons, calculates bullet stats based on upgrades,
/// and handles firing bullets at a rate determined by the current fire rate.
/// 
/// </summary>
public class WeaponManager : MonoBehaviour
{
    BulletSpawner bulletSpawner;
    UpgradeManager upgradeManager;
    WeaponSpawner weaponSpawner;


    public WeaponType defaultWeaponType;
    private List<Weapon> activeWeapons = new();
    private List<UpgradeData> activeUpgrades = new();
    private CachedStats cachedStats;
    private float fireTimer;


    void OnEnable() => UpgradeManager.OnUpgradeChanged += HandleUpgradeChange;
    void OnDisable() => UpgradeManager.OnUpgradeChanged -= HandleUpgradeChange;


    public void Start()
    {
        cachedStats = new CachedStats();
        bulletSpawner = ServiceLocator.Get<BulletSpawner>();
        upgradeManager = ServiceLocator.Get<UpgradeManager>();
        weaponSpawner = ServiceLocator.Get<WeaponSpawner>();
        RecalculateCachedStats();
    }

    void HandleUpgradeChange(UpgradeCategory category)
    {
        if (category == UpgradeCategory.Weapon)
        {
            activeUpgrades.Clear();
            upgradeManager.GetUpgrades(UpgradeCategory.Weapon).ForEach(upgrade =>
            {
                    activeUpgrades.Add(upgrade);
            });
            RecalculateCachedStats();
        }
    }



    // CachedStats holds all current weapon and bullet stats to make it easier to recreate them.
    // This will only updated when something changes.
    // This way we don't have to calculate updated stats every Shoot() called.
    public struct CachedStats
    {
        public WeaponType weaponType;
        public BulletType bulletType;
        public float bulletSpeed;
        public float bulletDamage;
        public float bulletRange;
        public float fireRate;
    }

    void RecalculateCachedStats()
    {
        cachedStats.weaponType = GetCurrentWeaponType();
        cachedStats.bulletType = GetCurrentBulletType();
        cachedStats.bulletSpeed = CalculateBulletSpeed();
        cachedStats.bulletDamage = CalculateBulletDamage();
        cachedStats.bulletRange = CalculateBulletRange();
        cachedStats.fireRate = CalculateFireRate();
    }


    public Weapon RequestWeapon()
    {
        Weapon w = weaponSpawner.SpawnWeaponFromType(cachedStats.weaponType);
        RegisterWeapon(w);
        return w;
    }

    public void RegisterWeapon(Weapon weapon)
    {
        if (!activeWeapons.Contains(weapon))
            activeWeapons.Add(weapon);
    }

    public void UnregisterWeapon(Weapon weapon)
    {
        if (activeWeapons.Contains(weapon))
            activeWeapons.Remove(weapon);
    }

    
    
    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / cachedStats.fireRate)
        {
            FireAllWeapons();
            fireTimer = 0f;
        }
    }

    void FireAllWeapons()
    {
        BulletType type = cachedStats.bulletType;
        type.stats.speed = cachedStats.bulletSpeed;
        type.stats.damage = cachedStats.bulletDamage;
        type.stats.range = cachedStats.bulletRange;

        foreach (var w in activeWeapons)
        {
            if (w == null) continue;
            Transform fp = w.GetFirePoint();
            if (fp == null) continue;

            bulletSpawner.FireBullet(type, fp.position);
        }
    }



    float CalculateBulletSpeed()
    {
        float speed = defaultWeaponType.bulletType.stats.speed;
        float totalUp = 1f;
        foreach (var up in activeUpgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.SpeedPercentage)
                totalUp += up.value;
        return speed * totalUp;
    }

    float CalculateBulletDamage()
    {
        float damage = defaultWeaponType.bulletType.stats.damage;
        float totalUp = 1f;
        foreach (var up in activeUpgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.DamagePercentage)
                totalUp += up.value;
        return damage * totalUp;
    }
    float CalculateBulletRange()
    {
        float range = defaultWeaponType.bulletType.stats.range;
        float totalUp = 1f;
        foreach (var up in activeUpgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.RangePercentage)
                totalUp += up.value;
        return range * totalUp;
    }


    float CalculateFireRate()
    {
        float rate = defaultWeaponType.fireRate;
        float totalUp = 1f;
        foreach (var up in activeUpgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.FireRatePercentage)
                totalUp += up.value;
        return rate * totalUp;
    }


    BulletType GetCurrentBulletType()
    {
        foreach (var up in activeUpgrades)
        {
            if (up.type is BulletTypeUpgrade bulletUpgrade)
                return bulletUpgrade.bulletType;
        }
        return defaultWeaponType.bulletType;
    }

    WeaponType GetCurrentWeaponType()
    {
        foreach (var up in activeUpgrades)
        {
            if (up.type is WeaponTypeUpgrade weaponUpgrade)
                return weaponUpgrade.weaponType;
        }
        return defaultWeaponType;
    }

}