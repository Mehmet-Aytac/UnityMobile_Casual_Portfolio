using UnityEngine;
using UnityEngine.InputSystem;

public class PivotController : MonoBehaviour
{
    public float speed = 5f;
    public float sideBounds = 28.0f;
    public float topBound = 15.0f;
    public float bottomBound = 2.0f;

    private float halfWidthOfFormation;
    private float halfLengthOfFormation;




    CharacterGroupManager characterGroupManager;
    CharacterSpawner characterSpawner;


    Vector2 moveInput;
    PlayerInput playerInput;



    void Start()
    {
        characterGroupManager = ServiceLocator.Get<CharacterGroupManager>();
        characterSpawner = ServiceLocator.Get<CharacterSpawner>();
        characterGroupManager.OnFormationSizeChanged += UpdateClamp;
    }

    void Awake()
    {
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }


    private void OnDisable()
    {
        characterGroupManager.OnFormationSizeChanged -= UpdateClamp;
    }

    private void UpdateClamp(float width, float length)
    {
        halfWidthOfFormation = width;
        halfLengthOfFormation = length;
    }









    // FOR TESTING PURPOSES ONLY
    public CharacterType characterType;

    // END FOR TESTING PURPOSES ONLY

    void Update()
    {
        // FOR TESTING PURPOSES ONLY
        if (playerInput.actions["TestAddKey"].triggered)
        {
            var posSpawn = new Vector3(0, 3.1f, 0);
            var go = characterSpawner.SpawnCharacterOfType(characterType, posSpawn);
            characterGroupManager.AddCharacter(go.GetComponent<Character>());
        }

        if (playerInput.actions["TestRemoveKey"].triggered)
        {
            Character randomCharacter = characterGroupManager.characters[Random.Range(0, characterGroupManager.characters.Count)];
            characterGroupManager.RemoveCharacter(randomCharacter);
            randomCharacter.Die();
        }

        // END FOR TESTING PURPOSES ONLY



        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime;

        transform.position += move;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -sideBounds + halfWidthOfFormation, sideBounds - halfWidthOfFormation);
        pos.z = Mathf.Clamp(pos.z, bottomBound + halfLengthOfFormation, topBound - halfLengthOfFormation);
        transform.position = pos;
    }

    
}