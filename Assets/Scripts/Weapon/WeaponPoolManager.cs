using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 
/// Manages object pools for different Weapon types, allowing efficient spawning and recycling of Weapon instances.
/// 
/// </summary>


public class WeaponPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolData
    {
        public WeaponType weaponType;
        public int defaultCapacity = 50;
        public int maxSize = 200;
    }

    public List<PoolData> pools;
    private Dictionary<int, ObjectPool<Weapon>> poolDict = new();

    public void Start()
    {
        foreach (var data in pools)
        {
            ObjectPool<Weapon> pool = new ObjectPool<Weapon>(
                createFunc: () => CreateWeapon(data),
                actionOnGet: w => w.gameObject.SetActive(true),
                actionOnRelease: w => w.gameObject.SetActive(false),
                actionOnDestroy: w => Destroy(w.gameObject),
                collectionCheck: false,
                defaultCapacity: data.defaultCapacity,
                maxSize: data.maxSize
            );

            poolDict[data.weaponType.idHash] = pool;

        }
        PrewarmPools();
    }



    public void PrewarmPools()
    {
        foreach (var data in pools)
        {
            if (!poolDict.TryGetValue(data.weaponType.idHash, out var pool)) continue;

            for (int i = 0; i < data.defaultCapacity; i++)
            {
                var w = pool.Get();
                pool.Release(w);
            }
        }
    }



    Weapon CreateWeapon(PoolData data)
    {
        Weapon w = Instantiate(data.weaponType.prefab).GetComponent<Weapon>();
        if (w == null)
        {
            Debug.LogError($"Prefab for {data.weaponType.id} has no Weapon component!");
            return null;
        }

        w.SetPool(poolDict[data.weaponType.idHash]);
        return w;
    }

    public Weapon SpawnWeapon(WeaponType weaponType)
    {
        if (!poolDict.TryGetValue(weaponType.idHash, out var pool))
        {
            Debug.LogError("WeaponPoolManager: No pool found for weapon type " + weaponType.id);
            return null;
        }

        Weapon w = pool.Get();
        return w;
    }
}