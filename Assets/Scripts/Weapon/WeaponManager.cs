using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;


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


    public WeaponType currentWeaponType;
    private List<Weapon> activeWeapons = new();
    private List<UpgradeData> activeUpgrades = new();
    private CachedStats cachedStats;
    private float fireTimer;

    private List<UpgradeData> simulatedUpgrades; // For DPS Calculator


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
        cachedStats.weaponType = GetCurrentWeaponType(activeUpgrades);
        cachedStats.bulletType = GetCurrentBulletType(activeUpgrades);
        cachedStats.bulletSpeed = CalculateBulletSpeed(currentWeaponType, activeUpgrades);
        cachedStats.bulletDamage = CalculateBulletDamage(currentWeaponType, activeUpgrades);
        cachedStats.bulletRange = CalculateBulletRange(currentWeaponType, activeUpgrades);
        cachedStats.fireRate = CalculateFireRate(currentWeaponType, activeUpgrades);
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



    float CalculateBulletSpeed(WeaponType type, List<UpgradeData> upgrades)
    {
        float speed = type.bulletType.stats.speed;
        float totalUp = 1f;
        foreach (var up in upgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.SpeedPercentage)
                totalUp += up.value;
        return speed * totalUp;
    }

    float CalculateBulletDamage(WeaponType type, List<UpgradeData> upgrades)
    {
        float damage = type.bulletType.stats.damage;
        float totalUp = 1f;
        foreach (var up in upgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.DamagePercentage)
                totalUp += up.value;
        return damage * totalUp;
    }
    float CalculateBulletRange(WeaponType type, List<UpgradeData> upgrades)
    {
        float range = type.bulletType.stats.range;
        float totalUp = 1f;
        foreach (var up in upgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.RangePercentage)
                totalUp += up.value;
        return range * totalUp;
    }


    float CalculateFireRate(WeaponType type, List<UpgradeData> upgrades)
    {
        float rate = type.fireRate;
        float totalUp = 1f;
        foreach (var up in upgrades)
            if (up.type.category == UpgradeType.UpgradeTypeCategory.FireRatePercentage)
                totalUp += up.value;
        return rate * totalUp;
    }


    BulletType GetCurrentBulletType(List<UpgradeData> upgrades)
    {
        foreach (var up in upgrades)
        {
            if (up.type is BulletTypeUpgrade bulletUpgrade)
                return bulletUpgrade.bulletType;
        }
        return currentWeaponType.bulletType;
    }

    WeaponType GetCurrentWeaponType(List<UpgradeData> upgrades)
    {
        foreach (var up in upgrades)
        {
            if (up.type is WeaponTypeUpgrade weaponUpgrade)
                return weaponUpgrade.weaponType;
        }
        return currentWeaponType;
    }



    /// <summary>
    /// Calculates and updates the damage per second (DPS) values for each row and column in the specified player
    /// formation.
    /// </summary>
    /// <remarks>This method resets the existing row and column DPS values before performing the calculation.</remarks>
    /// <param name="data">The player formation data containing character positions and arrays to receive the calculated row and column DPS
    /// values. Cannot be null, and its 'characters' property must not be null.</param>
    public void FillDPS(PlayerFormationData data)
    {
        WeaponType typeWeapon;

        if (data == null || data.characterCount == 0)
            return;

        if (data.upgrades == null)
        {
            simulatedUpgrades = activeUpgrades;
            data.upgrades = activeUpgrades;
        }
        else simulatedUpgrades = data.upgrades;

        if (data.weaponType == null)
        { 
            typeWeapon = currentWeaponType;
            data.weaponType = currentWeaponType;
        }
        else typeWeapon = data.weaponType;

        // Reset
        for (int r = 0; r < data.rows; r++) data.rowDPS[r] = 0f;
        for (int c = 0; c < data.cols; c++) data.colDPS[c] = 0f;



        // Precompute weapon stats from upgrades
        float damage = CalculateBulletDamage(typeWeapon, simulatedUpgrades);
        float fireRate = CalculateFireRate(typeWeapon, simulatedUpgrades);
        float speed = CalculateBulletSpeed(typeWeapon, simulatedUpgrades);
        float range = CalculateBulletRange(typeWeapon, simulatedUpgrades);

        float charDPS = damage * fireRate; // Single enemy DPS

        // Set rowRanges, rowDPS and colDPS length
        data.AllocateDPSArrays(data.rows, data.cols);


        // Calculate total DPS for each row
        for (int i = 0; i < data.rowDPS.Length; i++)
        {
            data.rowDPS[i] = charDPS * data.rowsCharacterCounts[i];
        }

        // Calculate total DPS for each column
        for (int i = 0; i < data.colDPS.Length; i++)
        {
            data.colDPS[i] = charDPS * data.colsCharacterCounts[i];
        }

        // Calculate ranges of each row. Pivot point range is default.
        for (int i = 0; i < data.rows; i++)
        {
            data.rowRanges[i] = data.rowDepth[i] + range;
        }

    }
}