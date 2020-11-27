using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Health bar of boss to shrink on damage")]
    public Image HealthBar;
    [Tooltip("Health controller of boss to attach actions to")]
    public HealthController HealthController;
    [Tooltip("Source of projectiles")]
    public Transform MuzzlePosition;
    [Tooltip("What boss should aim at")]
    public Transform Target;
    [Tooltip("Conductor object used to time actions")]
    public Conductor Conductor;
    [Tooltip("Audio Source to play sound effects from")]
    public AudioSource AudioSource;
    [Tooltip("Sound effect played when boss fires projectiles")]
    public AudioClip FireSFX;
    [Tooltip("Rendered object to dissolve when boss is killed")]
    public Dissolve Dissolve;
    [Tooltip("Time taken to dissolve boss on death")]
    public float DissolveTime = 2f;

    [Header("Combat")]
    [Tooltip("Projectiles fired by boss")]
    public Projectile Projectile;
    
    int mAmmoLeft = 8;
    float mBeatReloadStart = -10;
    float mTimeOfDeath = -10f;

    /// <summary> Get references and add actions. </summary>
    void Start()
    {
        HealthController.OnDamaged += OnDamaged;
        HealthController.OnDeath += OnDeath;
    }

    /// <summary> Update function. </summary>
    void Update()
    {
        // Dissolve out if dead
        float dissolveProgress = (Time.time - mTimeOfDeath) / DissolveTime;
        if (dissolveProgress < 1 && dissolveProgress > 0)
        {
            Dissolve.SetDissolved(dissolveProgress);
        }

        // Turn to face target
        Vector3 toTarget = Target.position - transform.position;
        toTarget.y = 0f;
        transform.rotation = Quaternion.LookRotation(toTarget);

        Fire();
    }

    /// <summary> Fire projectile if appropriate. </summary>
    void Fire()
    {
        // Reload if no ammo
        if (mAmmoLeft == 0)
        {
            if (Conductor.GetBeat() > mBeatReloadStart + 4)
            {
                mAmmoLeft = 8;
            }
            else
            {
                return;
            }
        }

        // Fire on half-beats
        if ((Conductor.GetBeat(2) - mAmmoLeft) % 2 == 0)
        {
            Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
            newProjectile.Owner = HealthController.gameObject;
            newProjectile.Damage = 5f;
            AudioSource.pitch = 0.95f + 0.1f * (mAmmoLeft % 2);
            AudioSource.PlayOneShot(FireSFX);
            mAmmoLeft--;
        }
        
        // Reload if no ammo
        if (mAmmoLeft == 0)
        {
            mBeatReloadStart = Conductor.GetBeat();
        }
    }

    /// <summary> Action called when target is damaged. </summary>
    void OnDamaged()
    {
        HealthBar.fillAmount = HealthController.NormalizedHealth;
    }
    
    /// <summary> Action called when target dies. </summary>
    void OnDeath()
    {
        HealthBar.fillAmount = 0f;
        mTimeOfDeath = Time.time;
        Destroy(gameObject, DissolveTime);
    }
}
