using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolData
    {
        public EnemyType enemyType;
        public int defaultCapacity = 50;
        public int maxSize = 200;
    }

    public List<PoolData> pools;

    private Dictionary<string, ObjectPool<Enemy>> poolDict = new();

    void Awake()
    {
        foreach (var data in pools)
        {
            ObjectPool<Enemy> pool = new ObjectPool<Enemy>(
                createFunc: () => CreateEnemy(data),
                actionOnGet: e => e.gameObject.SetActive(true),
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                collectionCheck: false,
                defaultCapacity: data.defaultCapacity,
                maxSize: data.maxSize
            );

            poolDict[data.enemyType.id] = pool;
        }
    }

    Enemy CreateEnemy(PoolData data)
    {
        Enemy e = Instantiate(data.enemyType.prefab).GetComponent<Enemy>();
        e.SetPool(poolDict[data.enemyType.id]);
        return e;
    }

    public void SpawnEnemy(EnemyType enemyType, Vector3 pos)
    {
        if (!poolDict.TryGetValue(enemyType.id, out var pool)) return;
        Enemy b = pool.Get();
        b.Initialize(enemyType, pos);
        b.transform.SetParent(SceneOrganizer.enemyRoot, false);
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
