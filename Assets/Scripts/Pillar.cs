using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar: MonoBehaviour
{
    [Header("General")]
    [Tooltip("Number of hits required to destroy")]
    public int Health = 1;
    public GameObject Effects;

    HealthController mHealthController;
    float mCrrtHealth;
    float mTimeLastHit = -10f;
    Vector3 mOriginalPos;

    /// <summary> Cache components. </summary>
    private void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        mHealthController.OnDeath += OnDamaged;
        mCrrtHealth = Health;
        mOriginalPos = transform.position;
    }

    private void Update()
    {
        Vector3 newPos = mOriginalPos;
        if (mCrrtHealth <= 0)
        {
            newPos.y -= (Time.time-mTimeLastHit) * 1f;
        }

        if (Time.time - mTimeLastHit < 0.5f || mCrrtHealth <= 0)
        {
            newPos.y += Mathf.Sin(Time.time * 10f + 0f) * 0.1f;
            newPos.z += Mathf.Sin(Time.time * 10f + 0.5f) * 0.1f;
            newPos.x += Mathf.Sin(Time.time * 10f + 1f) * 0.1f;
        }

        transform.position = newPos;
    }
    
    /// <summary> Destroy object. </summary>
    void Destroy()
    {
    }

    /// <summary> Remove health when bottle is damaged. We do this here instead of in the health controller because we want the damage to be binary. </summary>
    void OnDamaged()
    {
        mCrrtHealth -= 1;
        if (mCrrtHealth >= 0)
        {
            mTimeLastHit = Time.time;
        }
        if (mCrrtHealth == 0)
        {
            Effects.SetActive(true);
        }
    }
}
