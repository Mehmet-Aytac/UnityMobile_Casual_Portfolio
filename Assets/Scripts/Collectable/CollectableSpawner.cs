using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    CollectablePoolManager poolManager;
    public CollectableTypeSO[] collectableTypes;

    void Start()
    {
        poolManager = ServiceLocator.Get<CollectablePoolManager>();

        if (collectableTypes == null || collectableTypes.Length == 0)
        {
            Debug.LogError("CollectableSpawner: collectableTypes array is empty!");
            return;
        }
    }

    
}