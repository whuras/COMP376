﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLifeController : MonoBehaviour
{
    public int MultiplierHealAmount;
    
    private PlayerInterface mPlayerHUD;
    private PlayerController mPlayerController;
    private HealthController mHealthController;
    private bool mPlayerDied = false;
    
    // Start is called before the first frame update
    void Start()
    {
        mPlayerHUD = gameObject.GetComponentInChildren<PlayerInterface>();
        mHealthController = GetComponent<HealthController>();
        mPlayerController = GetComponent<PlayerController>();

        mHealthController.OnDamaged += OnPlayerDamaged;
        mHealthController.OnDeath += OnPlayerDeath;
        mPlayerController.OnMultiplierIncrement += OnMultiplierIncreaseHealPlayer;
    }

    public void OnMultiplierIncreaseHealPlayer()
    {
        mHealthController.Heal(MultiplierHealAmount);
        mPlayerHUD.SetHealthDisplayed(mHealthController.NormalizedHealth);
        Debug.Log(mHealthController.NormalizedHealth);
    }
    
    public void OnPlayerDamaged()
    {
        mPlayerHUD.SetHealthDisplayed(mHealthController.NormalizedHealth);
    }

    public void OnPlayerDeath()
    {
        if (!mPlayerDied)
        {
            Instantiate(Resources.Load("GameOverScreen"));
            mPlayerDied = true;
        }
    }
}
