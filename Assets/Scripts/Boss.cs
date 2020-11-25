using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public Image HealthBar;
    public HealthController HealthController;
    public Animator Animator;
    public Projectile Projectile;
    public Transform MuzzlePosition;
    public Transform Target;
    public Conductor Conductor;
    public AudioSource AudioSource;
    public AudioClip FireSFX;
    float lastShot = -10f;
    int mAmmoLeft = 8;
    float mBeatReloadStart = -10;

    /// <summary> Get references and add actions. </summary>
    void Start()
    {
        HealthController.OnDamaged += OnDamaged;
        HealthController.OnDeath += OnDeath;
    }

    void Update()
    {
        // Turn to face target
        transform.rotation = Quaternion.LookRotation(Target.position - transform.position);

        
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

        //On-beat shot
        bool onBeat = Conductor.GetTimeToBeat() > 0;
        if (onBeat && mAmmoLeft % 2 == 0)
        {
            Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
            newProjectile.Damage = 5f;
            newProjectile.Owner = HealthController.gameObject;
            mAmmoLeft--;
            AudioSource.PlayOneShot(FireSFX);
        }
        // Off-beat shot
        if (!onBeat && mAmmoLeft % 2 == 1)
        {
            Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
            newProjectile.Damage = 5f;
            newProjectile.Owner = HealthController.gameObject;
            mAmmoLeft--;
            AudioSource.PlayOneShot(FireSFX);
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
        Animator.Play("Death");
        Destroy(gameObject, 3f);
    }
}
