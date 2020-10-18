using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public float Health = 10f;
    public UnityAction OnDamaged;
    public UnityAction OnDeath;

    public void TakeDamage(float damage)
    {
        Health -= damage;

        if (Health > 0)
        {
            OnDamaged();
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        OnDeath();
    }
}
