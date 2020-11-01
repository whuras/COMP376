using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private Conductor mConductor;
    private Damageable mDamageable;
    void Start()
    {
        mDamageable = gameObject.GetComponent<Damageable>();
        mDamageable.OnDamaged += OnDamaged;
        mDamageable.OnDeath += OnDeath;

        mConductor = GameObject.Find("Conductor").GetComponent<Conductor>();
    }

    private void Update()
    {
        // magic number
        if (Mathf.Abs(mConductor.GetTimeToBeat()) < 0.025 && mDamageable.IsAlive)
        {
            //OnBeat();    
        }
    }

    public void OnBeat()
    {
        gameObject.GetComponent<Animator>().Play("Pulse", -1, 0f);
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
