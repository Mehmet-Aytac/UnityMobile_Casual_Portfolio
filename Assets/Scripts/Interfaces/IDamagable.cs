using UnityEngine;

public interface IDamagable
{
    public float currentHealth { get; }
    public float maxHealth { get; }

    public delegate void TakeDamageEvent (float damageAmount);
    public event TakeDamageEvent OnTakeDamage;

    public delegate void DeathEvent (Vector3 position);
    public event DeathEvent OnDeath;

    public void TakeDamage(float damageAmount);
}
