using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponFireType
{
    [Tooltip("Player must press fire button to fire weapon once")]
    Manual,
    [Tooltip("Player can hold down fire button to fire weapon repeatedly")]
    Automatic,
    [Tooltip("Player must hold down fire button to charge up shot that is released on button release")]
    Charge
}

public enum WeaponBulletType
{
    [Tooltip("Weapon uses raycasts to determine objects hit")]
    Raycast,
    [Tooltip("Weapon fires physical bullets that must collide with an object to damage it")]
    Projectile
}

public class Weapon : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Position of weapon muzzle")]
    public Transform MuzzlePosition;

    [Header("Weapon Mechanics")]
    [Tooltip("The weapon fire type determines how the weapon fires")]
    public WeaponFireType FireType = WeaponFireType.Manual;
    [Tooltip("The weapon bullet type determines the type of bullet fired by weapon")]
    public WeaponBulletType BulletType = WeaponBulletType.Raycast;
    [Tooltip("Projectile fired by weapon while in projectile mode")]
    public Projectile Projectile;
    [Tooltip("Base damage per bullet")]
    public float Damage = 5f;
    [Tooltip("Range of weapon")]
    public float Range = 100f;
    [Tooltip("Time between successive shots")]
    public float Period = 1f;
    [Tooltip("Ammo that can be loaded at any time")]
    public uint ClipSize = 10;
    [Tooltip("How close to the beat the player needs to shoot for the shot to be considered on beat. (1 is always, 0 is never).")]
    public float BeatTolerance;

    [Header("Gunfire Effects")]
    [Tooltip("Particle effects emitted from muzzle on fire")]
    public GameObject GunfireVisualEffects;
    [Tooltip("Audio clips played on gunfire")]
    public AudioClip[] GunfireSoundEffects;
    [Tooltip("Amplitude of gun recoil after firing")]
    [Range(0f, 90f)]
    public float RecoilAngle = 30;
    [Tooltip("Amplitude of gun kickback after firing")]
    public float RecoilKickback = 0.25f;
    [Tooltip("Duration of recoil")]
    public float RecoilDuration = 0.5f;
    [Tooltip("Normalized time to apex of recoil")]
    [Range(0f, 1f)]
    public float RecoilApex = 0.1f;

    [Header("Reload Effects")]
    [Tooltip("Duration of recoil")]
    public float ReloadTime = 1f;
    [Tooltip("Number of spins done during reload")]
    public uint SpinCount = 3;
    [Tooltip("Sound effect played on reload")]
    public AudioClip ReloadSFX;

    [Header("Weapon Reticle")]
    [Tooltip("Transform holding reticle UI elements")]
    public RectTransform Reticle;
    [Tooltip("Canvas Group holding hitmarker UI elements")]
    public CanvasGroup Hitmarker;
    [Tooltip("Amplitude of change in reticle size")]
    public float ReticlePulseAmplitude;  
    [Tooltip("Duration of time during which hitmarker sits on screen")]
    public float HitmarkerDuration;    

    public static UnityAction OnSuccessfulHit;
    public static UnityAction OnUnsuccessfulHit;
    
    private Conductor mConductor;
    private AudioSource mAudioSource;
    
    float mTimeLastReload = -10f;
    float mTimeLastShot = -10f;
    float mTimeLastSuccessfulShot = -10f;
    bool mIsReloading = false;
    uint mAmmoLeft;
    public uint AmmoLeft => mAmmoLeft;

    /// <summary> Get referenced objects. </summary>
    void Start()
    {
        mConductor = Conductor.GetActiveConductor();
        mAudioSource = GetComponent<AudioSource>();
        mAmmoLeft = ClipSize;
    }

    /// <summary> Animate weapon every frame. </summary>
    void Update()
    {
        AnimateReticle();
        if (mIsReloading)
        {
            AnimateReload();
            return;
        }
        AnimateRecoil();
    }

    /// <summary> Animate weapon reticle. </summary>
    void AnimateReticle()
    {
        // Animate reticle pulse
        float timeToBeat = mConductor.GetTimeToBeat();
        float scale = 1f + ReticlePulseAmplitude * Mathf.Abs(timeToBeat);
        Reticle.localScale = Vector3.one * scale;

        // Fade out hitmarker
        float t = 1f - Mathf.Clamp01((Time.time - mTimeLastSuccessfulShot) / HitmarkerDuration);
        Hitmarker.alpha = t;
    }

    /// <summary> Animate recoil of weapon according to time since last shot. </summary>
    void AnimateRecoil()
    {
        // t measures normalized time (0 to 1) from start of recoil animation to end of recoil animation.
        float t = Mathf.Clamp01((Time.time - mTimeLastShot)/RecoilDuration);
        float recoil = (t < RecoilApex) 
            ? 1 - Mathf.Pow(1-t/RecoilApex, 2)
            : 1 - Mathf.Pow((RecoilApex-t)/(1-RecoilApex), 2);

        // Rotate muzzle upward according to recoil at current time.
        Vector3 crrtRotation = transform.localEulerAngles;
        crrtRotation.x = -RecoilAngle * recoil;
        transform.localEulerAngles = crrtRotation;

        // Push stock into shoulder according to recoil at current time.
        Vector3 crrtPosition = transform.localPosition;
        crrtPosition.z = -0.25f*recoil;
        transform.localPosition = crrtPosition;
    }

    /// <summary> Fires weapon according to weapon fire type and inputs if appropriate. </summary>
    /// <param name="fireDown">Was fire button pressed this frame.</param>
    /// <param name="fireHeld">Was fire button held this frame.</param>
    /// <param name="fireReleased">Was fire button released this frame.</param>
    /// <returns> True if weapon was fired. False otherwise. </returns>
    public bool ReceiveFireInputs(bool fireDown, bool fireHeld, bool fireReleased)
    {
        switch (FireType)
        {
            case WeaponFireType.Manual:
                if (fireDown)
                {
                    return Fire();
                }
                break;
            case WeaponFireType.Automatic:
                if (fireHeld)
                {
                    return Fire();
                }
                break;
        }
        return false;
    }

    /// <summary> Fire bullet of appropriate type if enough time has passed since last fire and enough ammo remains. Add weapon SFX. </summary>
    bool Fire()
    {
        // Check if weapon can be fired. Return early if weapon was last fired too recently or ammo is missing
        if (mIsReloading || mAmmoLeft == 0 || mTimeLastShot + Period > Time.time)
        {
            return false;
        }

        // Fire weapon according to bullet type.
        switch (BulletType)
        {
            case WeaponBulletType.Raycast:
                FireRaycast();
                break; 
            case WeaponBulletType.Projectile:
                FireProjectile();
                break;
        }
        mTimeLastShot = Time.time;
        mAmmoLeft -= 1;
        PlayWeaponFireEffects();

        return true;
    }

    /// <summary> Fire raycast. </summary>
    void FireRaycast()
    {
        // Fire raycast.
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            // Check for damageable objects hit by raycast.
            HealthController target = hit.collider.gameObject.GetComponent<HealthController>();
            if (target != null)
            {
                // Deal damage.
                float damageGiven = Damage * (IsShotOnBeat() ? 1 : 0);
                target.TakeDamage(damageGiven);
        
                if (IsShotOnBeat())
                {
                    OnSuccessfulShot(target);
                }
                else
                {
                    OnUnsuccessfulShot();
                }
            }
            else
            {
                OnUnsuccessfulShot();
            }
        }
        else
        {
            OnUnsuccessfulShot();
        }
    }

    /// <summary> Fire projectile. </summary>
    void FireProjectile()
    {
        Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
        float damageGiven = Damage * (IsShotOnBeat() ? 1 : 0);
        newProjectile.Damage = damageGiven;
        newProjectile.Owner = gameObject;
        
        if (IsShotOnBeat())
        {
            newProjectile.OnSuccessfulImpact += OnSuccessfulShot;
            newProjectile.OnUnsuccessfulImpact += OnUnsuccessfulShot;
        }
        else
        {
            OnUnsuccessfulShot();
        }
    }

    /// <summary> Animates effects that play when gun is fired. </summary>
    void PlayWeaponFireEffects()
    {
        // Visual effects
        GameObject fx = Instantiate(GunfireVisualEffects, MuzzlePosition);
        Destroy(fx, 0.2f);

        // Sound effects
        mAudioSource.PlayOneShot(GunfireSoundEffects[UnityEngine.Random.Range(0, GunfireSoundEffects.Length-1)]);
    }

    /// <summary> Fill weapon clip. </summary>
    public void Reload()
    {
        // Return early if already reloaded or full of ammo
        if (mIsReloading || mAmmoLeft == ClipSize)
        {
            return;
        }

        // Start reload otherwise
        mIsReloading = true;
        mTimeLastReload = Time.time;
        mAudioSource.PlayOneShot(ReloadSFX, 5f);
    }

    /// <summary> Animate weapon reload if applicable. </summary>
    void AnimateReload()
    {
        // t measures normalized time (0 to 1) from start of reload animation to end of reload animation.
        float t = (Time.time - mTimeLastReload) / ReloadTime;
        // Complete reload if animation is complete.
        if (t > 1)
        {
            if (mIsReloading)
            {
                mIsReloading = false;
                mAmmoLeft = ClipSize;
            }
            return;
        }

        // Determine rotation of weapon.
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        float y = t < 0.5
            ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        float rotation = -360f * SpinCount * y;

        // Rotate weapon.
        Vector3 crrtRotation = transform.localEulerAngles;
        crrtRotation.x = rotation;
        transform.localRotation = Quaternion.Euler(new Vector3(rotation, 0f, 0f));
    }

    /// <summary> Respond to successful shots. </summary>
    void OnSuccessfulShot(HealthController target)
    {
        mTimeLastSuccessfulShot = Time.time;
        OnSuccessfulHit?.Invoke();
    }

    /// <summary> Respond to unsuccessful shots. </summary>
    void OnUnsuccessfulShot()
    {
        OnUnsuccessfulHit?.Invoke();
    }

    /// <summary> Determine factor by which weapon damage should be scaled using time to nearest beat. </summary>
    float ComputeDamageModifier()
    {
        return Mathf.Pow(1 - Mathf.Pow(mConductor.GetTimeToBeat() / 0.5f, 4), 8);
    }

    /// <summary> Determine whether or not shot is on beat according to beat tolerance defined in properties. </summary>
    bool IsShotOnBeat()
    {
        return Mathf.Abs(mConductor.GetTimeToBeat()) <= BeatTolerance / 2;
    }
}
