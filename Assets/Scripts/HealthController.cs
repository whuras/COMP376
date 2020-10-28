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
            OnDeath();
        }
        else
        {
            OnDamaged();
        }
    }

    public void Heal(int health)
    {
        if (healthAmount + health > healthAmountMaximum)
        {
            healthAmount = healthAmountMaximum;
        }
        
        OnHealed();
    }

    public float GetHealthNormalized()
    {
        return (float)healthAmount / healthAmountMaximum;
    }
}