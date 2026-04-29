using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Enemy parentEnemy;

    public void Hit(int damage)
    {
        if (parentEnemy != null)
        {
            parentEnemy.TakeDamage(damage);
        }
    }
}