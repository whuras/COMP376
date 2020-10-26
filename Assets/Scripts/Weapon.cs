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
    public Projectile Projectile;
    public Transform MuzzlePosition;
    public GameObject Owner;

    public uint maxAmmo = 6;
    public uint mAmmoLeft = 6;
    float mTimeLastShot = 0f;

    void Start()
    {
        
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
