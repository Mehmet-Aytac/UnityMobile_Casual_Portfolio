using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float minHorizontalPosition = -16f;
    public float maxHorizontalPosition = 16f;



    PivotController pivotController;
    Vector3 playerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pivotController = FindFirstObjectByType<PivotController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Follow the player position on the x axis between -16 and 16
        playerTransform = pivotController.GetComponent<Transform>().position;
        transform.position = new Vector3(Mathf.Clamp(playerTransform.x,minHorizontalPosition, maxHorizontalPosition), transform.position.y, transform.position.z);
    }
}
