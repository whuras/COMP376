using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a debug script and can be safely deleted at any time

public class DamageTestTarget : MonoBehaviour
{
    public PlayerController Player;
    public HealthController healthController;

    void Start()
    {
        healthController = gameObject.GetComponent<HealthController>();
        healthController.OnDamaged += HealthController_OnDamaged;
        healthController.OnDamaged += HealthController_OnDeath;
    }

    void HealthController_OnDamaged()
    {
        Player.healthController.TakeDamage(10);
    }

    void HealthController_OnDeath()
    {

    }
}
