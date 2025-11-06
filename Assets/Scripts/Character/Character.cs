using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Animator))]
public class Character : MonoBehaviour
{
    WeaponManager weaponManager;
    [HideInInspector] public Animator animator;
    public float moveSpeed = 5f;

    Vector3 targetPosition;
    Weapon currentWeapon;
    public CharacterType type;
    readonly int speedHash = Animator.StringToHash("Speed");
    Transform weaponAnchor;


    // Reference to the object pool
    private ObjectPool<Character> pool;
    

    // Method to set the pool reference
    public void SetPool(ObjectPool<Character> characterPool)
    {
        pool = characterPool;
    }

    private void Start()
    {
        weaponManager = ServiceLocator.Get<WeaponManager>();
        AskForWeapon();
    }

    public void Initialize(CharacterType characterType, Vector3 spawnLocation)
    {
        
        type = characterType;
        transform.position = spawnLocation;
        weaponAnchor = transform.Find("WeaponAnchor");
    }


    public void ResetState()
    {
        targetPosition = transform.position;
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        Vector3 dir = targetPosition - transform.position;
        //animator.SetFloat(speedHash, dir.magnitude);

    }

    void AskForWeapon()
    {
        currentWeapon = weaponManager.RequestWeapon();
        currentWeapon.transform.SetParent(weaponAnchor, false);
    }

    public void MoveToFormation(Vector3 position)
    {
        targetPosition = position;
    }

    public void Die()
    {
        if (pool != null)
            pool.Release(this);
        else gameObject.SetActive(false);
    }

}