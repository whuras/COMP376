using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackingModel : MonoBehaviour
{
    public UnityEvent ActionOnAttack;
    public void OnAttack()
    {
        ActionOnAttack?.Invoke();
    }
}
