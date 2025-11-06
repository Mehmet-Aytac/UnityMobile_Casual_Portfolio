using UnityEngine;
using UnityEngine.Pool;

public class Collectable : MonoBehaviour
{
    private CollectableTypeSO type;
    private ObjectPool<Collectable> pool;
    private float currentHealth;

    public void SetPool(ObjectPool<Collectable> collectablePool)
    {
        pool = collectablePool;
    }

    public void Initialize(CollectableTypeSO collectableType, Vector3 spawnPosition)
    {
        type = collectableType;
        transform.position = spawnPosition;
        currentHealth = type.stats.maxHealth;
        gameObject.SetActive(true);
    }

   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Apply reward logic here, for example:
        // GameManager.Instance.AddCurrency(type.stats.value);

        pool.Release(this);
    }
}
