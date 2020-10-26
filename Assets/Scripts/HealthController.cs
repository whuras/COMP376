using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    public UnityAction OnDamaged;
    public UnityAction OnHealed;
    public UnityAction OnDeath;

    public int healthAmount;
    public int healthAmountMaximum;

    public void TakeDamage(int damage)
    {
        healthAmount -= damage;
        if (healthAmount < 0)
        {
            healthAmount = 0;
        }
        else
        {
            OnDamaged();
        }

        if (healthAmount == 0)
        {
            OnDeath();
        }
    }

    public void Heal(int health)
    {
        healthAmount += health;
        if (healthAmount > healthAmountMaximum)
        {
            healthAmount = healthAmountMaximum;
        }
        else
        {
            OnHealed();
        }
    }

    public float GetHealthNormalized()
    {
        return (float)healthAmount / healthAmountMaximum;
    }
}