using UnityEngine;

[CreateAssetMenu(fileName = "CollectableType", menuName = "Collectables/New Collectable Type")]
public class CollectableTypeSO : ScriptableObject
{
    public string id;
    public CollectableStats stats;
    public GameObject prefab;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}