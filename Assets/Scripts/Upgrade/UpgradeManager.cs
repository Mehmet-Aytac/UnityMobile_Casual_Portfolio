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
    private readonly Dictionary<UpgradeCategory, List<UpgradeData>> upgrades = new();

    public static event Action<UpgradeCategory> OnUpgradeChanged;

    void Awake()
    {
        foreach (UpgradeCategory cat in System.Enum.GetValues(typeof(UpgradeCategory)))
            upgrades[cat] = new List<UpgradeData>();
    }

    public void AddUpgrade(UpgradeCategory category, UpgradeData upgrade)
    {

        // if it's a type-change upgrade, remove the previous one
        var list = upgrades[category];
        if (upgrade.type is BulletTypeUpgrade || upgrade.type is WeaponTypeUpgrade)
        {
            for (int i = upgrades.Count - 1; i >= 0; i--)
            {
                if (list[i].type is BulletTypeUpgrade || list[i].type is WeaponTypeUpgrade)
                    list.RemoveAt(i);
            }
        }

        // add the new upgrade and notify listeners
        upgrades[category].Add(upgrade);
        OnUpgradeChanged?.Invoke(category);
    }

    public List<UpgradeData> GetUpgrades(UpgradeCategory category)
    {
        return upgrades[category];
    }
}
