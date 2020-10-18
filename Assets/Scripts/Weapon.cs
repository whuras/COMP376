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
    public float Damage = 5f;
    public float Period = 1f;

    float mTimeLastShot = 0f;
    uint mAmmoLeft = 10;

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

        return true;
    }

    void FireRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            Target target = hit.collider.gameObject.GetComponent<Target>();
            if (target != null)
            {
                target.TakeHit(this);
            }
        }
    }

    void FireProjectile()
    {
    }
}
