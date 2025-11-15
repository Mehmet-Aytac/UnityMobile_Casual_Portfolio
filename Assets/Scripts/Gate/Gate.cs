using UnityEngine;
using UnityEngine.Pool;

public class Gate : MonoBehaviour
{
    private GateType type;

    // Reference to the object pool
    private ObjectPool<Gate> pool;

    // Method to set the pool reference
    public void SetPool(ObjectPool<Gate> gatePool)
    {
        pool = gatePool;
    }

    // Release from pool when gate "used"
    public void Use()
    {
        pool.Release(this);
    }



    public void Initialize(GateType gateType)
    {
        type = gateType;
    }


    private void Update()
    {
        // Move the enemy forward
        transform.Translate(type.stats.speed * Time.deltaTime * Vector3.forward);


        // NEED TO FIND A BETTER WAY TO HANDLE ALL THESE OUT OF BOUNDS CHECKS

        // Check if the enemy has exceeded its range(preset z value in this case) then return it to the pool
        if (transform.position.z <= -30)
        {
            pool.Release(this);
        }
    }
}
