using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolData
    {
        public BulletType bulletType;
        public int defaultCapacity = 50;
        public int maxSize = 200;
    }

    public List<PoolData> pools;

    private Dictionary<string, ObjectPool<Bullet>> poolDict = new();

    void Awake()
    {
        foreach (var data in pools)
        {
            ObjectPool<Bullet> pool = new ObjectPool<Bullet>(
                createFunc:() => CreateBullet(data),
                actionOnGet: b => b.gameObject.SetActive(true),
                actionOnRelease: b => b.gameObject.SetActive(false),
                actionOnDestroy: b => Destroy(b.gameObject),
                collectionCheck: false,
                defaultCapacity: data.defaultCapacity,
                maxSize: data.maxSize
            );

            poolDict[data.bulletType.id] = pool;

            // Prewarm without re-instantiating inside CreateBullet again
            for (int i = 0; i < data.defaultCapacity; i++)
            {
                var bullet = pool.Get();
                pool.Release(bullet);
            }
        }
    }

    Bullet CreateBullet(PoolData data)
    {
        Bullet b = Instantiate(data.bulletType.prefab).GetComponent<Bullet>();
        if (b == null)
        {
            Debug.LogError($"Prefab for {data.bulletType.id} has no Bullet component!");
            return null;
        }

        b.SetPool(poolDict[data.bulletType.id]);
        return b;
    }

    public void SpawnBullet(BulletType bulletType, Vector3 pos)
    {
        if (!poolDict.TryGetValue(bulletType.id, out var pool)) return;
        Bullet b = pool.Get();
        b.Initialize(bulletType, pos);
        Transform parent = GetOrCreateGroup(SceneOrganizer.bulletRoot, bulletType.id);
        b.transform.SetParent(parent, false);

    }



    /// <summary>
    /// Retrieves an existing transform group by its identifier or creates a new one if it does not exist.
    /// </summary>
    /// <remarks>If the group with the specified identifier does not exist, a new GameObject is created with
    /// the given identifier, and its transform is set as a child of the specified root transform.</remarks>
    /// <param name="root">The root transform under which to search for or create the group.</param>
    /// <param name="id">The unique identifier of the group to find or create.</param>
    /// <returns>The transform of the existing group if found; otherwise, a new transform group with the specified identifier.</returns>
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
