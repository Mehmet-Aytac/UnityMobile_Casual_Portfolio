using System;
using UnityEngine;

/// <summary>
/// 
/// Bootstrapper class responsible for initializing and registering manager components at the start of the game.
/// 
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    [Header("Manager Prefabs or Scene Objects")]
    public GameObject[] managerPrefabs;

    void Awake()
    {
        foreach (var entry in managerPrefabs)
        {
            if (entry == null) continue;

            // If entry is a Scene object -> register only
            if (entry.scene.IsValid())
            {
                RegisterAll(entry);
                continue;
            }

            // If entry is a prefab -> instantiate and register
            GameObject instance = Instantiate(entry);
            instance.name = entry.name;
            DontDestroyOnLoad(instance);

            RegisterAll(instance);
        }

        ServiceLocator.Get<WeaponPoolManager>().PrewarmPools();
    }

    void RegisterAll(GameObject obj)
    {
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            Type type = comp.GetType();
            if (!ServiceLocator.TryGet(type, out _))
            {
                ServiceLocator.Register(type, comp);
            }
        }
    }
}