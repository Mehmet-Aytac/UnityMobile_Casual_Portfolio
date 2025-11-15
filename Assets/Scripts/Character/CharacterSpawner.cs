using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    CharacterPoolManager poolManager;
    public CharacterType[] characterTypes;



    void Start()
    {
        poolManager = ServiceLocator.Get<CharacterPoolManager>();
        if (characterTypes == null || characterTypes.Length == 0)
        {
            Debug.LogError("CharacterSpawner: characterTypes array is empty!");
            return;
        }


        // Example: spawn random type
        var randomType = characterTypes[Random.Range(0, characterTypes.Length)];
        poolManager.SpawnCharacter(randomType, new Vector3(0f, 0f, 0f));
    }

    public Character SpawnCharacterOfType(CharacterType type, Vector3 position)
    {
        Character c = poolManager.SpawnCharacter(type, position);
        return c;
    }

}