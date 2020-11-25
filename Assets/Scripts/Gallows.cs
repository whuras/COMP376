using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gallows : MonoBehaviour
{
    private HealthController h;
    // Start is called before the first frame update
    void Start()
    {
        h = GetComponent<HealthController>();
        h.OnDeath += GallowsEscape;
    }

    void GallowsEscape()
    {
        
    }
}
