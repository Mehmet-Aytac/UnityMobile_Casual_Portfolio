using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CharacterPoolManager : MonoBehaviour
{
    public List<PoolData> pools;

    private Dictionary<int, ObjectPool<Character>> poolDict = new();

    CharacterGroupManager groupManager;


    [System.Serializable]
    public class PoolData
    {
        public CharacterType characterType;
        public int defaultCapacity = 100;
        public int maxSize = 375;
    }


    private void Start()
    {
        groupManager = ServiceLocator.Get<CharacterGroupManager>();
        foreach (var data in pools)
        {
            ObjectPool<Character> pool = new ObjectPool<Character>(
                createFunc: () => CreateCharacter(data),
                actionOnGet: OnCharacterGet,
                actionOnRelease: OnCharacterRelease,
                actionOnDestroy: c => Destroy(c.gameObject),
                collectionCheck: false,
                defaultCapacity: data.defaultCapacity,
                maxSize: data.maxSize
            );

            poolDict[data.characterType.idHash] = pool;

            // Prewarm without re-instantiating inside CreateCharacter again
            for (int i = 0; i < data.defaultCapacity; i++)
            {
                var character = pool.Get();
                pool.Release(character);
            }
        }
    }


    void OnCharacterGet(Character c)
    {
        c.gameObject.SetActive(true);
        groupManager?.AddCharacter(c);
    }

    void OnCharacterRelease(Character c)
    {
        groupManager?.RemoveCharacter(c);
        c.ResetState();
        c.gameObject.SetActive(false);
    }

    Character CreateCharacter(PoolData data)
    {
        Character c = Instantiate(data.characterType.prefab).GetComponent<Character>();
        if (c == null)
        {
            Debug.LogError($"Prefab for {data.characterType.id} has no Character component!");
            return null;
        }

        c.SetPool(poolDict[data.characterType.idHash]);
        return c;
    }

    public Character SpawnCharacter(CharacterType characterType, Vector3 pos)
    {
        if (!poolDict.TryGetValue(characterType.idHash, out var pool)) return null;
        Character c = pool.Get();
        c.Initialize(characterType, pos);
        Transform parent = GetOrCreateGroup(SceneOrganizer.characterRoot, characterType.id);
        c.transform.SetParent(parent, false);
        return c;

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
