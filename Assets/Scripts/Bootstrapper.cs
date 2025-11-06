using System;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [Header("Manager Prefabs (Assign in Assets)")]
    public GameObject[] managerPrefabs;

    void Awake()
    {
        // 1. Instantiate all managers
        foreach (var prefab in managerPrefabs)
        {
            if (prefab == null) continue;

            GameObject instance = Instantiate(prefab);
            instance.name = prefab.name; // remove (Clone)
            DontDestroyOnLoad(instance);

            // 2. Register all components in the prefab
            MonoBehaviour[] components = instance.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                Type type = comp.GetType();
                if (!ServiceLocator.TryGet(type, out _))
                    ServiceLocator.Register(type, comp);
            }
        }

        
        ServiceLocator.Get<WeaponPoolManager>().PrewarmPools();
    }
}
