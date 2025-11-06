using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CollectablePoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolData
    {
        public CollectableTypeSO collectableType;
        public int defaultCapacity = 50;
        public int maxSize = 200;
    }

    public List<PoolData> pools;
    private Dictionary<int, ObjectPool<Collectable>> poolDict = new();

    void Start()
    {
        foreach (var data in pools)
        {
            ObjectPool<Collectable> pool = new ObjectPool<Collectable>(
                createFunc: () => CreateCollectable(data),
                actionOnGet: c => c.gameObject.SetActive(true),
                actionOnRelease: c => c.gameObject.SetActive(false),
                actionOnDestroy: c => Destroy(c.gameObject),
                collectionCheck: false,
                defaultCapacity: data.defaultCapacity,
                maxSize: data.maxSize
            );

            poolDict[data.collectableType.idHash] = pool;

            // Prewarm
            for (int i = 0; i < data.defaultCapacity; i++)
            {
                var c = pool.Get();
                pool.Release(c);
            }
        }
    }

    Collectable CreateCollectable(PoolData data)
    {
        Collectable c = Instantiate(data.collectableType.prefab).GetComponent<Collectable>();
        if (c == null)
        {
            Debug.LogError($"Prefab for {data.collectableType.id} has no Collectable component!");
            return null;
        }

        c.SetPool(poolDict[data.collectableType.idHash]);
        return c;
    }

    public void SpawnCollectable(CollectableTypeSO collectableType, Vector3 pos)
    {
        if (!poolDict.TryGetValue(collectableType.idHash, out var pool)) return;
        Collectable c = pool.Get();
        c.Initialize(collectableType, pos);
        Transform parent = GetOrCreateGroup(SceneOrganizer.collectableRoot, collectableType.id);
        c.transform.SetParent(parent, false);
    }

    Transform GetOrCreateGroup(Transform root, string id)
    {
        Transform group = root.Find(id);
        if (group == null)
        {
            GameObject g = new GameObject(id);
            g.transform.SetParent(root);
            group = g.transform;
        }
        return group;
    }
}
