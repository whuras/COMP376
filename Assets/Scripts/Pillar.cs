using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar: MonoBehaviour
{
    [Tooltip("Number of hits required to destroy")]
    public int Health = 1;
    [Tooltip("Speed of pillar sink")]
    public float SinkSpeed = 2f;
    [Tooltip("Frequency of pillar shake animation")]
    public float RumbleFrequency = 10f;
    [Tooltip("Amplitude of pillar shake animation")]
    public float RumbleAmplitude = 0.1f;
    [Tooltip("Particle effects to enable when pillar is being destroyed")]
    public ParticleSystem Particles;

    HealthController mHealthController;
    Vector3 mOriginalPos;
    float mCrrtHealth;
    float mTimeLastHit = -10f;

    /// <summary> Cache components. </summary>
    private void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        mHealthController.OnDeath += OnDamaged;
        mCrrtHealth = Health;
        mOriginalPos = transform.position;
    }

    /// <summary> Update function. </summary>
    private void Update()
    {
        // Update height of pillar while pillar is sinking.
        Vector3 newPos = mOriginalPos;
        if (mCrrtHealth <= 0)
        {
            newPos.y -= (Time.time - mTimeLastHit) * SinkSpeed;
            Destroy(transform.parent.gameObject, 7f);
        }

        // Shake pillar on damage.
        if (Time.time - mTimeLastHit < 0.5f || mCrrtHealth <= 0)
        {
            newPos.y += Mathf.Sin(Time.time * RumbleFrequency + 0f)   * RumbleAmplitude;
            newPos.z += Mathf.Sin(Time.time * RumbleFrequency + 0.5f) * RumbleAmplitude;
            newPos.x += Mathf.Sin(Time.time * RumbleFrequency + 1f)   * RumbleAmplitude;
            transform.position = newPos;
        }
        else
        {
            Particles.Stop();
        }
    }

    /// <summary> Remove health when bottle is damaged. We do this here instead of in the health controller because we want the damage to be binary. </summary>
    void OnDamaged()
    {
        mCrrtHealth -= 1;
        if (mCrrtHealth >= 0)
        {
            mTimeLastHit = Time.time;
            Particles.Play();
        }
    }
}
