using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Health bar image object")]
    public Image HealthBarFill;
    [Tooltip("Health bar image object")]
    public Image HealthBarDamage;
    [Tooltip("Current health text object")]
    public Text CurrentHealthText;
    [Tooltip("Current ammo text object")]
    public Text CurrentAmmoText;
    
    [Header("Animations")]
    [Tooltip("Time taken by health bar change animation")]
    public float HealthAnimationDuration;

    float mTimeOfLastHealthUpdate = -10f;
    float mCurrentHealth = 1f;
    float mTargetHealth = 1f;

    /// <summary> Animate interface </summary>
    void Update()
    {
        AnimateHealthBar();
    }

    /// <summary> Animate health bar fill and depletion. </summary>
    void AnimateHealthBar()
    {
        // Determine normalized progress of animation
        float t = (Time.time - mTimeOfLastHealthUpdate) / HealthAnimationDuration;
        // Animate if animation is in progress
        if (t < 1)
        {
            HealthBarDamage.fillAmount = Mathf.Lerp(mCurrentHealth, mTargetHealth, mTimeOfLastHealthUpdate);
        }
    }

    /// <summary> Update ammunition shown by interface. </summary>
    /// <param name="crrtAmmo"> Current ammunition in clip </param>
    /// <param name="maxAmmo"> Size of clip </param>
    public void SetAmmoDisplayed(uint crrtAmmo, uint maxAmmo)
    {
        CurrentAmmoText.text = string.Format("{0} / {1}", crrtAmmo, maxAmmo);
    }

    /// <summary> Update health shown by interface. </summary>
    /// <param name="currentWeapon"> Updated health normalized from 0 to 1 </param>
    public void SetHealthDisplayed(float normalizedHealth)
    {
        HealthBarFill.fillAmount = normalizedHealth;
        mCurrentHealth = mTargetHealth;
        mTargetHealth = normalizedHealth;
        CurrentHealthText.text = string.Format("{0}", (int) (100 * normalizedHealth));
        mTimeOfLastHealthUpdate = Time.time;
    }
}
