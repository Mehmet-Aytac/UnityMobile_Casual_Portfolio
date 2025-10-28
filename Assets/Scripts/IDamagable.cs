using UnityEngine;

public interface IDamagable
{
    public int currentHealth { get; }
    public int maxHealth { get; }

    public delegate void TakeDamageEvent (int damageAmount);
    public event TakeDamageEvent OnTakeDamage;

    public delegate void DeathEvent (Vector3 position);
    public event DeathEvent OnDeath;

    public void TakeDamage(int damageAmount);
}
