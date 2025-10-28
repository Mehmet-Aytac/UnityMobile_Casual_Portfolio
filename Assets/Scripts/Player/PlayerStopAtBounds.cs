using UnityEngine;

public class PlayerStopAtBounds : MonoBehaviour
{

    public float sideBounds = 13.0f;
    public float topBound = 15.0f;
    public float bottomBound = 4.0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -sideBounds)
        {
            transform.position = new Vector3(-sideBounds, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > sideBounds)
        {
            transform.position = new Vector3(sideBounds, transform.position.y, transform.position.z);
        }
        if (transform.position.z < bottomBound)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, bottomBound);
        }
        else if (transform.position.z > topBound)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, topBound);
        }
    }
}
