using System.Collections.Generic;

public class PlayerFormationData
{
    // Character Group Formation
    public FormationShape shape;
    public int rows;
    public int cols;
    public int characterCount;
    public int[] rowsCharacterCounts;
    public int[] colsCharacterCounts;
    public float[] rowDepth;

    // Character
    public CharacterType characterType;

    // DPS
    public float characterDPS;
    public float[] rowDPS;
    public float[] colDPS;
    public float[] rowRanges;

    // Weapon
    public WeaponType weaponType;
    public BulletType bulletType;
    public float speed;

    public List<UpgradeData> upgrades;

    public void AllocateDPSArrays(int r, int c)
    {
        rowRanges = new float[r];
        rowDPS = new float[r];
        colDPS = new float[c];
    }
}
