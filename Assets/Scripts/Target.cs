using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public bool returnsDamage;
    public PlayerController Player;
    public HealthController healthController;

    void Start()
    {
        healthController = gameObject.GetComponent<HealthController>();
        healthController.OnDamaged += HealthController_OnDamaged;
        healthController.OnDeath += HealthController_OnDeath;
    }

    void HealthController_OnDamaged()
    {
        if (returnsDamage)
        {
            Player.healthController.TakeDamage(10);
        }
        else
        {
            gameObject.GetComponent<Animator>().Play("Damaged", -1, 0f);
        }
    }

    void HealthController_OnDeath()
    {
        gameObject.GetComponent<Animator>().Play("Dead");
    }
}
