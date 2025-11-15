using UnityEngine;

[CreateAssetMenu(fileName = "Upgrades", menuName = "Upgrades/New Upgrade Type")]
public class UpgradeType : ScriptableObject
{
    public enum UpgradeTypeCategory
    {
        ChangeWeaponType,
        ChangeBulletType,
        ChangeCharacterType,
        CharacterAmount,
        DamagePercentage,
        SpeedPercentage,
        RangePercentage,
        FireRatePercentage
    }

    public string id;
    public float value;
    public UpgradeTypeCategory category;


    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}