using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "Weapons/New Weapon Type")]
public class WeaponType : ScriptableObject
{
    public string id;
    public float fireRate;
    public BulletType bulletType;
    public GameObject prefab;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}