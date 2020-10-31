using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponFireType
{
    Manual,
    Automatic,
    Charge
}

public enum WeaponBulletType
{
    Raycast,
    Projectile
}

public class Weapon : MonoBehaviour
{
    public WeaponFireType FireType = WeaponFireType.Manual;
    public WeaponBulletType BulletType = WeaponBulletType.Raycast;
    public float Range = 100f;
    public int Damage;
    public float Period = 1f;
    public float MaxRecoil = 60;
    public Projectile Projectile;
    public Transform MuzzlePosition;
    public GameObject Owner;

    public uint mMaxAmmo = 6;
    public uint mAmmoLeft = 6;
    float mTimeLastShot = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        RecoilAnimation();
    }

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

    bool Fire()
    {
        if (mAmmoLeft == 0 || mTimeLastShot + Period > Time.time)
        {
            return false;
        }
        mTimeLastShot = Time.time;

        switch (BulletType)
        {
            case WeaponBulletType.Raycast:
                FireRaycast();
                break;
            case WeaponBulletType.Projectile:
                FireProjectile();
                break;
        }

        mAmmoLeft--;
        return true;
    }

    void RecoilAnimation()
    {
        Vector3 crrtRotation = transform.localEulerAngles;
        crrtRotation.x = -20f*Mathf.Exp(5f*(mTimeLastShot - Time.time));
        transform.localEulerAngles = crrtRotation;

        Vector3 crrtPosition = transform.localPosition;
        crrtPosition.z = -0.5f*Mathf.Exp(5f*(mTimeLastShot - Time.time));
        transform.localPosition = crrtPosition;
    }

    void FireRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            HealthController target = hit.collider.gameObject.GetComponent<HealthController>();
            if (target != null)
            {
                target.TakeDamage(Damage);
            }
        }
    }

    void FireProjectile()
    {
        Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
        newProjectile.Owner = Owner;
        newProjectile.Damage = Damage;
    }
}
