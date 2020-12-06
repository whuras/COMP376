using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public abstract Transform Target { get; set; }
    public abstract HealthController HealthController { get; }
    public abstract Conductor Conductor { get; set; }
}
