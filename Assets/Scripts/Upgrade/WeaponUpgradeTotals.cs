[System.Serializable]
public struct WeaponUpgradeTotals
{
    public float damagePercent;   // e.g. 0.25f means +25%
    public float speedPercent;
    public float rangePercent;
    public float fireRatePercent;
    public BulletType bulletType;
    public WeaponType weaponType;
}
