using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// Controls the pivot point for the player's character group, handling movement input.
/// Sole listener for the player inputs.
/// 
/// </summary>
public class PivotController : MonoBehaviour
{
    public float speed = 5f;
    public float sideBounds = 28.0f;
    public float bottomBound = 2.0f;
    public Vector3 posSpawn = new(0, 3.1f, 0);


    private float halfWidthOfFormation;


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
    }









 // FOR TESTING PURPOSES ONLY
    public CharacterType characterType;

 // END FOR TESTING PURPOSES ONLY

    void Update()
    {
 // FOR TESTING PURPOSES ONLY

        if (playerInput.actions["TestAddKey"].triggered)
        {
            characterSpawner.SpawnCharacterOfType(characterType, posSpawn);
        }


        if (playerInput.actions["TestAddAllKey"].triggered)
        {
            int maxRows = characterGroupManager.MaxRows;
            int maxCols = characterGroupManager.MaxCols;

            for (int i = 0; i < maxRows*maxCols; i++)
            {
                characterSpawner.SpawnCharacterOfType(characterType, posSpawn);
            }
        }


        if (playerInput.actions["TestRemoveKey"].triggered)
        {
            Character randomCharacter = characterGroupManager.characters[Random.Range(0, characterGroupManager.characters.Count)];
            randomCharacter.Die();
        }


        if (playerInput.actions["SquareFormation"].triggered)
        {
            characterGroupManager.SetFormationShape(CharacterGroupManager.FormationShape.Square);
        }
        if (playerInput.actions["HorizontalFormation"].triggered)
        {
            characterGroupManager.SetFormationShape(CharacterGroupManager.FormationShape.Horizontal);
        }
        if (playerInput.actions["VerticalFormation"].triggered)
        {
            characterGroupManager.SetFormationShape(CharacterGroupManager.FormationShape.Vertical);
        }

 // END FOR TESTING PURPOSES ONLY



        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(moveInput.x, 0, 0) * speed * Time.deltaTime;

        transform.position += move;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -sideBounds + halfWidthOfFormation, sideBounds - halfWidthOfFormation);
        transform.position = pos;
    }

    
}