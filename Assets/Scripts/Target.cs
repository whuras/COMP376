using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    void Start()
    {
        Damageable damageable = gameObject.GetComponent<Damageable>();
        damageable.OnDamaged += OnDamaged;
        damageable.OnDeath += OnDeath;
    }

    void OnDamaged()
    {
        gameObject.GetComponent<Animator>().Play("Damaged", -1, 0f);
    }

    void OnDeath()
    {
        gameObject.GetComponent<Animator>().Play("Dead");
    }
}
