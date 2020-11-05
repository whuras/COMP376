using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable: MonoBehaviour
{
    public GameObject destroyedVersion;
    public HealthController healthController;

    private void Start()
    {
        healthController = gameObject.GetComponent<HealthController>();
        healthController.OnDamaged += OnDamaged;
    }

    void OnDamaged()
    {
        Instantiate(destroyedVersion, transform.position, transform.rotation);
        Destroy(gameObject);

        if (tag == "Whiskey")
        {
            FindObjectOfType<TutorialManager>().UpdateBottleCount(1);
        }
    }
}
