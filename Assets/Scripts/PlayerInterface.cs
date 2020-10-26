using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    // Health
    private const float SHRINK_TIMER_MAX = 1f;
    private float damagedHealthShrinkTimer;
    public Image healthBarFill;
    public Image healthBarDamage;
    public Text currentHealthText;

    // Ammo
    public Text currentAmmoText;

    // Specials

    private void Awake()
    {
        healthBarDamage.fillAmount = healthBarFill.fillAmount;
    }

    void Update()
    {
        damagedHealthShrinkTimer -= Time.deltaTime;
        if (damagedHealthShrinkTimer < 0)
        {
            if (healthBarFill.fillAmount < healthBarDamage.fillAmount)
            {
                float shrinkSpeed = 0.5f;
                healthBarDamage.fillAmount -= shrinkSpeed * Time.deltaTime;
            }

        }
    }

    public void SetHealthBar(float normalizedHealth)
    {
        healthBarFill.fillAmount = normalizedHealth;
    }

    public void SetAmmoCount(Weapon currentWeapon)
    {
        currentAmmoText.text = currentWeapon.maxAmmo.ToString();
    }

    public void UpdateHealthBar(bool isHeal, float normalizedHealth)
    {
        SetHealthBar(normalizedHealth);
        currentHealthText.text = (normalizedHealth * 100).ToString();

        if (isHeal)
        {
            healthBarDamage.fillAmount = healthBarFill.fillAmount;
        }
        else
        {
            damagedHealthShrinkTimer = SHRINK_TIMER_MAX;
        }
    }

    public void UpdateAmmoCount(Weapon currentWeapon)
    {
        currentAmmoText.text = currentWeapon.mAmmoLeft.ToString();
    }
}
