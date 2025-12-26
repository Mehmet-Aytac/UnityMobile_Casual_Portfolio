using UnityEngine;

public interface IBulletHitHandler
{
    void OnBulletHit(Bullet bullet, Collider other);
}
