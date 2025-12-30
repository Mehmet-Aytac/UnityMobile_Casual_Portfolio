using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeCategory
{
    Weapon,
    Character,
    Other
}

public class UpgradeManager : MonoBehaviour
{
    // Separate storage for meta (permanent) and per-run (runtime) upgrades
    private readonly Dictionary<UpgradeCategory, List<UpgradeData>> permanentUpgrades = new();
    private readonly Dictionary<UpgradeCategory, List<UpgradeData>> runtimeUpgrades = new();

    public static event Action<UpgradeCategory> OnUpgradeChanged;

    void Awake()
    {
        foreach (UpgradeCategory cat in Enum.GetValues(typeof(UpgradeCategory)))
        {
            permanentUpgrades[cat] = new List<UpgradeData>();
            runtimeUpgrades[cat] = new List<UpgradeData>();
        }

        // Hook for loading permanent upgrades from disk (implement later)
        LoadPermanentUpgrades();
    }

    [System.Serializable]
    public struct WeaponUpgradeTotals
    {
        public float damagePercent;
        public float speedPercent;
        public float rangePercent;
        public float fireRatePercent;
    }

    private WeaponUpgradeTotals weaponTotalsCache;
    private bool weaponTotalsDirty = true;

    public WeaponUpgradeTotals GetWeaponTotals()
    {
        if (weaponTotalsDirty)
            RebuildWeaponTotals();

        return weaponTotalsCache;
    }

    /// <summary>
    /// Recalculates and updates the cached totals for all weapon-related upgrades.
    /// Includes both permanent and runtime upgrades.
    /// </summary>
    void RebuildWeaponTotals()
    {
        weaponTotalsCache = default;

        // Permanent weapon upgrades
        var perm = permanentUpgrades[UpgradeCategory.Weapon];
        for (int i = 0; i < perm.Count; i++)
        {
            AccumulateWeaponUpgrade(perm[i].type);
        }

        // Runtime weapon upgrades
        var run = runtimeUpgrades[UpgradeCategory.Weapon];
        for (int i = 0; i < run.Count; i++)
        {
            AccumulateWeaponUpgrade(run[i].type);
        }

        weaponTotalsDirty = false;
    }

    void AccumulateWeaponUpgrade(UpgradeType up)
    {
        switch (up.category)
        {
            case UpgradeType.UpgradeTypeCategory.DamagePercentage:
                weaponTotalsCache.damagePercent += up.value;
                break;
            case UpgradeType.UpgradeTypeCategory.SpeedPercentage:
                weaponTotalsCache.speedPercent += up.value;
                break;
            case UpgradeType.UpgradeTypeCategory.RangePercentage:
                weaponTotalsCache.rangePercent += up.value;
                break;
            case UpgradeType.UpgradeTypeCategory.FireRatePercentage:
                weaponTotalsCache.fireRatePercent += up.value;
                break;
        }
    }

    /// <summary>
    /// Existing API: this now acts as "add runtime upgrade".
    /// </summary>
    public void AddUpgrade(UpgradeCategory category, UpgradeData upgrade)
    {
        AddRuntimeUpgrade(category, upgrade);
    }

    /// <summary>
    /// Adds a runtime (per-run) upgrade to the specified category.
    /// </summary>
    public void AddRuntimeUpgrade(UpgradeCategory category, UpgradeData upgrade)
    {
        var list = runtimeUpgrades[category];

        if (upgrade.type is BulletTypeUpgrade)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].type is BulletTypeUpgrade)
                    list.RemoveAt(i);
            }
        }
        else if (upgrade.type is WeaponTypeUpgrade)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].type is WeaponTypeUpgrade)
                    list.RemoveAt(i);
            }
        }

        list.Add(upgrade);

        if (category == UpgradeCategory.Weapon)
            weaponTotalsDirty = true;

        OnUpgradeChanged?.Invoke(category);
    }

    /// <summary>
    /// Adds a permanent (meta) upgrade to the specified category.
    /// </summary>
    public void AddPermanentUpgrade(UpgradeCategory category, UpgradeData upgrade)
    {
        var list = permanentUpgrades[category];

        if (upgrade.type is BulletTypeUpgrade)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].type is BulletTypeUpgrade)
                    list.RemoveAt(i);
            }
        }
        else if (upgrade.type is WeaponTypeUpgrade)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].type is WeaponTypeUpgrade)
                    list.RemoveAt(i);
            }
        }

        list.Add(upgrade);

        if (category == UpgradeCategory.Weapon)
            weaponTotalsDirty = true;

        SavePermanentUpgrades();
        OnUpgradeChanged?.Invoke(category);
    }

    /// <summary>
    /// Clears all runtime upgrades for all categories (used when a run ends).
    /// </summary>
    public void ClearRuntimeUpgrades()
    {
        foreach (UpgradeCategory cat in Enum.GetValues(typeof(UpgradeCategory)))
        {
            runtimeUpgrades[cat].Clear();
            OnUpgradeChanged?.Invoke(cat);
        }

        weaponTotalsDirty = true;
    }

    /// <summary>
    /// Returns combined permanent + runtime upgrades for the category.
    /// This keeps the old GetUpgrades contract for WeaponManager and others.
    /// </summary>
    public List<UpgradeData> GetUpgrades(UpgradeCategory category)
    {
        var perm = permanentUpgrades[category];
        var run = runtimeUpgrades[category];

        var result = new List<UpgradeData>(perm.Count + run.Count);
        result.AddRange(perm);
        result.AddRange(run);
        return result;
    }

    public List<UpgradeData> GetPermanentUpgrades(UpgradeCategory category)
    {
        return permanentUpgrades[category];
    }

    public List<UpgradeData> GetRuntimeUpgrades(UpgradeCategory category)
    {
        return runtimeUpgrades[category];
    }

    void SavePermanentUpgrades()
    {
        // TODO: serialize permanentUpgrades to disk (store IDs/values, not ScriptableObject refs).
    }

    void LoadPermanentUpgrades()
    {
        // TODO: deserialize and rebuild permanentUpgrades from saved data.
    }
}
