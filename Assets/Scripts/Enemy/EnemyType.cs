using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemies/New Enemy Type")]
public class EnemyType : ScriptableObject
{
    public string id;
    public EnemyStats stats;
    public GameObject prefab;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}