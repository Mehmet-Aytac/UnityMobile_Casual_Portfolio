using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed = 5.0f;


    // Reference to the PlayerInput start
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private Vector2 move;

    void Awake()
    {
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }

    // Reference to the PlayerInput end


    // Update is called once per frame
    void Update()
    {

        // Read the movement input
        move = playerInput.actions["Move"].ReadValue<Vector2>();

        // If there is movement, call Move Method
        if (move != Vector2.zero) { Move(move); }
    }



    // Move method to move the player
    void Move(Vector2 move)
    {
        Vector3 movement = speed * Time.deltaTime * new Vector3(move.x, 0, move.y);
        transform.Translate(movement);
    }
}
