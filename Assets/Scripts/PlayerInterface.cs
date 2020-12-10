using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Actual health bar that changes instantly")]
    public Image HealthBarFront;
    [Tooltip("Health bar behind actual health that drags behind it")]
    public Image HealthBarBack;
    [Tooltip("Dash Icon")]
    public Image DashIcon;
    [Tooltip("Current health text object")]
    public Text CurrentHealthText;
    [Tooltip("Current ammo text object")]
    public Text CurrentAmmoText;
    
    
    public Text CurrentScoreText;
    public Text CurrentMultiplierText;
    public Text CurrentStreakText;

    [Header("Animations")]
    [Tooltip("Speed at which health bar grows/shrinks to represent current health")]
    public float HealthAnimationSpeed = 0.5f;
    [Tooltip("Delay between change to player health and animation of back health bar")]
    public float HealthAnimationDelay = 0.5f;

    // Health Bar Variables
    float mTimeOfLastHealthUpdate = -10f;
    float mCurrentHealth = 1f;
    float mTargetHealth = 1f;

    // Dash Icon Variables
    float mFillSpeed = 1f;


    /// <summary> Animate interface </summary>
    void Update()
    {
        RefillDashIcon();
        AnimateHealthBar();
    }

    /// <summary> Animate health bar fill and depletion. </summary>
    void AnimateHealthBar()
    {
        // Return early if aniamtion should not be playing.
        if (Time.time - mTimeOfLastHealthUpdate < HealthAnimationDelay)
            return;
        // Get direction of health animation.
        float delta = mTargetHealth - mCurrentHealth;
        if (delta > 0)
        {
            HealthBarBack.fillAmount = mCurrentHealth += HealthAnimationSpeed * Time.deltaTime;
            if (mCurrentHealth > mTargetHealth)
            {
                HealthBarBack.fillAmount = mCurrentHealth = mTargetHealth;
                mTimeOfLastHealthUpdate = 3.40282347E+38F;
            }
        }
        else
        {
            HealthBarBack.fillAmount = mCurrentHealth -= HealthAnimationSpeed * Time.deltaTime;
            if (mCurrentHealth < mTargetHealth)
            {
                HealthBarBack.fillAmount = mCurrentHealth = mTargetHealth;
                mTimeOfLastHealthUpdate = 3.40282347E+38F;
            }
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
        CurrentHealthText.text = string.Format("{0}", (int)(100 * normalizedHealth + 0.5));
        HealthBarFront.fillAmount = normalizedHealth;
        mTargetHealth = normalizedHealth;
        mTimeOfLastHealthUpdate = Time.time;
    }

    public void SetScoreDisplayed(int score)
    {
        CurrentScoreText.text = $"{score}";
    }

    public void SetMultiplierDisplayed(int multiplier)
    {
        CurrentMultiplierText.text = $"{multiplier}";
    }

    public void SetShotStreakDisplayed(int streak)
    {
        CurrentStreakText.text = $"Streak: {streak}";
    }

    /// <summary> Set the Dash Icon fill amount to 0. </summary>
    public void ResetDashIcon()
    {
        DashIcon.fillAmount = 0;
    }

    public void SpawnScorePopup(int score)
    {
        GameObject scorePopup = GameObject.Instantiate((GameObject)Resources.Load("ScorePopup"), CurrentScoreText.transform);
        ScoreAddition scoreAddition = scorePopup.GetComponent<ScoreAddition>();
        scoreAddition.SetScore(score);
    }
    
    public void RefillDashIcon()
    {
        if (DashIcon.fillAmount < 1)
        {
            DashIcon.fillAmount += mFillSpeed * Time.deltaTime;
        }
    }
}