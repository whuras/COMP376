using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float Health = 10f;

    public void TakeHit(Weapon weapon)
    {
        Health -= weapon.Damage;
        Debug.Log(Health);
    }
}
