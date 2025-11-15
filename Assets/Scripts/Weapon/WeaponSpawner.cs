using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    WeaponPoolManager poolManager;
    public WeaponType[] weaponTypes;


    void Start()
    {
        poolManager = ServiceLocator.Get<WeaponPoolManager>();

        if (weaponTypes == null || weaponTypes.Length == 0)
        {
            Debug.LogError("WeaponSpawner: weaponTypes array is empty!");
            return;
        }
    }


    public Weapon SpawnWeaponFromType(WeaponType weaponType)
    {
        if (weaponType == null)
        {
            Debug.LogError("WeaponSpawner: weaponType is null!");
            return null;
        }

        Weapon weapon = poolManager.SpawnWeapon(weaponType);
        return weapon;
    }

}
