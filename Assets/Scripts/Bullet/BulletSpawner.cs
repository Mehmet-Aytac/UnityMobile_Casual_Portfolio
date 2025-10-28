using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public BulletPoolManager poolManager;
    public BulletType[] bulletTypes;

    void Start()
    {
        if (bulletTypes == null || bulletTypes.Length == 0)
        {
            Debug.LogError("BulletSpawner: bulletTypes array is empty!");
            return;
        }


        // Example: spawn random type
        var randomType = bulletTypes[Random.Range(0, bulletTypes.Length)];
        poolManager.SpawnBullet(randomType, new Vector3(0f, 0f, 0f));
    }
}
