using UnityEngine;

[CreateAssetMenu(fileName = "CharacterType", menuName = "Characters/New Character Type")]
public class CharacterType : ScriptableObject
{
    public string id;
    public float verticalSize;
    public float horizontalSize;
    public Vector3 weaponSocket;
    public GameObject prefab;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}