using UnityEngine;

[CreateAssetMenu(fileName = "BulletType", menuName = "Bullets/New Bullet Type")]
public class BulletType: ScriptableObject
{
    public string id;
    public BulletStats stats;
    public GameObject prefab;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}