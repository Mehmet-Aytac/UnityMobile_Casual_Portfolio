using UnityEngine;
using UnityEngine.Pool;


/// <summary>
///  Responsible for keeping a firePoint Transform to be used when firing and Registering/unregistering itself in the WeaponManager
/// </summary>

public class Weapon : MonoBehaviour
{
    WeaponManager weaponManager;
    [SerializeField] Transform firePoint;

    // Reference to the object pool
    private ObjectPool<Weapon> pool;

    // Method to set the pool reference
    public void SetPool(ObjectPool<Weapon> weaponPool)
    {
        pool = weaponPool;
    }


    public void Initialize()
    {
        weaponManager = ServiceLocator.Get<WeaponManager>();
    }


    // Register and unregister with WeaponManager
    void OnEnable()
    {
        if (weaponManager != null)
            weaponManager.RegisterWeapon(this);
    }

    void OnDisable()
    {
        if (weaponManager != null)
            weaponManager.UnregisterWeapon(this);
    }



    // Send firePoint
    public Transform GetFirePoint()
    {
        return firePoint;
    }

    
}