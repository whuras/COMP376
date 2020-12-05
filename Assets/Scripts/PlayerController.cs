﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Transform rooting cameras")]
    public Transform CameraRoot;
    [Tooltip("Transform rooting weapons")]
    public Transform WeaponSocket;
    [Tooltip("Heads up display")]
    public PlayerInterface PlayerHUD;
    [Tooltip("Mouse sensitivity accross horizontal and vertical axes")]
    public Vector2 MouseSensitivity;

    [Header("Player Movement")]
    [Tooltip("Player walking speed")]
    public float CharacterSpeedNormal;
    [Tooltip("Player running speed")]
    public float CharacterSpeedRunning;
    [Tooltip("Magnitude of player jump velocity")]
    public float JumpSpeed;
    [Tooltip("Acceleration of gravity on player")]
    public float GravityAcceleration;

    [Header("Weapons")]
    [Tooltip("Weapons held by player")]
    public List<Weapon> Weapons;
    [Tooltip("Weapons bob amplitude")]
    public float WeaponBobAmplitude;
    [Tooltip("Weapons bob frequency")]
    public float WeaponBobFrequency;
    [Tooltip("Speed of weapon bob change with resepct to player speed")]
    public float WeaponBobSharpness;

    [Header("Camera")]
    [Tooltip("UP and DOWN look limit, the angle is symmetric for the top and bottom parts of the screen")]
    public float CameraClampAngleX;

    [Header("Special Effects")]
    [Tooltip("Foot step sound effects for walking on wood")]
    public AudioClip[] WoodFootStepSoundEffects;

    [Header("Abilities")]
    [Tooltip("Speed of Dash ability")]
    public float DashSpeed = 40f;
    [Tooltip("Duration of dash ability")]
    public float DashDuration = 0.3f;
    [Tooltip("Factor by which time scale is multiplied during slowdown")]
    public float SlowdownFactor = 0.5f;

    [Header("Scoreboard")] 
    public int ScorePerHit;
    
    // Disable Toggles (for cutscenes)
    bool mDisableMovement;
    bool mDisableWeapons;

    //Other game objects
    Conductor mConductor;

    // Components
    CharacterController mCharacterController;
    AudioSource mAudioSource;

    // Movement
    Vector3 mCharacterVelocity = new Vector3(0f, 0f, 0f);
    float mTimeLastJump = -10f;
    bool mIsGrounded;
    bool mIsRunning = false;

    // Animation
    int mParityOfNextBeat = 0;
    float mCurrentWeaponBobFactor;
    float mWeaponBobTime = 0f;

    // Weaponry
    Weapon mCrrtWeapon;
    int mCrrtWeaponIndex = 0;

    // Abilities
    float mTimeDashStart = -10f;
    Vector3 mDashVelocity;
    
    // Scoreboard
    int mScore = 0;
    int mMultiplier = 1;
    int mHitStreak = 0;

    /// <summary> Get referenced objects. </summary>
    void Start()
    {
        // Equip default weapon if provided
        if (Weapons.Any())
        {
            EquipWeapon(0);
        }

        // Initialize References
        mConductor = FindObjectOfType<Conductor>();
        mCharacterController = GetComponent<CharacterController>();
        mAudioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;

        // Disable Toggles turned off by default
        mDisableMovement = false;
        mDisableWeapons = false;
        
        // Setup weapon/score update delegates
        Weapon.OnSuccessfulHit += IncrementHitStreak;
        Weapon.OnSuccessfulHit += IncrementScore;
        Weapon.OnSuccessfulHit += ComputeMultiplier;
        Weapon.OnUnsuccessfulHit += ResetHitStreak;
        Weapon.OnUnsuccessfulHit += ComputeMultiplier;
        
    }

    /// <summary> Equip the weapon in inventory at a specified index. </summary>
    /// <param name="index"> Inventory index of weapon to be equipped. </param>
    void EquipWeapon(int index)
    {
        // Check for out-of-bound indices. Wrap them around (list should be circular).
        index = (index + Weapons.Count) % Weapons.Count;

        // Remove previously equiped weapon.
        if (mCrrtWeapon != null)
        {
            Destroy(mCrrtWeapon.gameObject);
        }

        // Equip new weapon.
        mCrrtWeaponIndex = index;
        mCrrtWeapon = Instantiate(Weapons[index], WeaponSocket, false);
    }

    /// <summary> Receive input and update player state accordingly. </summary>
    void Update()
    {
        CheckIfGrounded();
        if (!mDisableMovement)
        {
            HandleMovement();
        }

        HandleAiming();

        if (!mDisableWeapons)
        {
            HandleWeapons();
            HandleWeaponBob();
            HandleAbilities();
        }

        DisplayScore();
    }

    /// <summary> Delay physics updates to syncronize with physics system. </summary>
    void FixedUpdate()
    {
        mCharacterController.Move(mCharacterVelocity * Time.deltaTime);
    }

    void DisplayScore()
    {
        PlayerHUD.SetScoreDisplayed(mScore);
        PlayerHUD.SetMultiplierDisplayed(mMultiplier);
        PlayerHUD.SetShotStreakDisplayed(mHitStreak);
    }

    void IncrementScore()
    {
        int scoreToAdd = ScorePerHit * mMultiplier;
        mScore += scoreToAdd;
        PlayerHUD.SpawnScorePopup(scoreToAdd);
    }

    void ComputeMultiplier()
    {
        mMultiplier = (int)Mathf.Pow(2, (int)(mHitStreak / 10));
    }
    void IncrementHitStreak()
    {
        mHitStreak++;
    }

    void ResetHitStreak()
    {
        mHitStreak = 0;
    }
    
    /// <summary> Update whether or not player is grounded. </summary>
    void CheckIfGrounded()
    {
        mIsGrounded = mTimeLastJump + 0.2f < Time.time
                    && Physics.Raycast(transform.position, Vector3.down, 1.6f);
    }

    /// <summary> Play footstep sound effect depending on ground material and player speed. </summary>
    void PlayFootStep(float speed)
    {
        // Play no footsteps while dashing or no grounded.
        if (!mIsGrounded || Time.time < mTimeDashStart + DashDuration)
        {
            return;
        }
        // Play footsteps on beat.
        int stepsPerBeat = mIsRunning ? 3 : 2;
        if (mConductor.GetBeat(stepsPerBeat) % stepsPerBeat == mParityOfNextBeat)
        {
            mAudioSource.PlayOneShot(WoodFootStepSoundEffects[mParityOfNextBeat]);
            mParityOfNextBeat = (mParityOfNextBeat + 1) % stepsPerBeat;
        }
    }

    /// <summary> Receive player movement input and respond to it. </summary>
    void HandleMovement()
    {
        // WALKING/RUNNING
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        mIsRunning = Input.GetAxisRaw("Run") != 0;
        float speed = Mathf.Lerp(CharacterSpeedNormal, CharacterSpeedRunning, Input.GetAxis("Run"));
        moveDirection.Normalize();
        moveDirection = transform.TransformDirection(moveDirection);

        // JUMPING
        float verticalVelocity = 0f;
        if (mIsGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                mTimeLastJump = Time.time;
                verticalVelocity = JumpSpeed;
            }
            else if (mCharacterVelocity.magnitude > 0.1)
            {
                PlayFootStep(mCharacterVelocity.magnitude);
            }
        }
        else
        {
            verticalVelocity = mCharacterVelocity.y - GravityAcceleration * Time.deltaTime;
        }
        mCharacterVelocity = moveDirection * speed + Vector3.up * verticalVelocity;
    }

    /// <summary> Receive player mouse input and respond to it. </summary>
    void HandleAiming()
    {
        // AIMING (LEFT/RIGHT)
        Vector3 characterRotation = transform.localEulerAngles;
        float angleDelta = Input.GetAxis("Mouse X") * MouseSensitivity.x * Time.deltaTime;
        characterRotation.y += angleDelta;
        transform.localEulerAngles = characterRotation;

        // AIMING (UP/DOWN)
        Vector3 cameraRotation = CameraRoot.transform.localEulerAngles;
        angleDelta = Input.GetAxis("Mouse Y") * MouseSensitivity.y * Time.deltaTime;

        float newCameraRotationX = cameraRotation.x - angleDelta;
        if (newCameraRotationX > CameraClampAngleX && newCameraRotationX < (360 - CameraClampAngleX))
        {
            cameraRotation.x = newCameraRotationX > 180 ? 360 - CameraClampAngleX : CameraClampAngleX;
        }
        else
        {
            cameraRotation.x = newCameraRotationX;
        }

        CameraRoot.transform.localEulerAngles = cameraRotation;
    }

    /// <summary> Receive weapon input and respond to it. </summary>
    void HandleWeapons()
    {
        // Handle weapon fire
        mCrrtWeapon.ReceiveFireInputs(
            Input.GetButtonDown("Fire1"),
            Input.GetButton("Fire1"),
            Input.GetButtonUp("Fire1"));

        // Handle weapon reload
        if (Input.GetButtonDown("Reload"))
        {
            mCrrtWeapon.Reload();
        }

        // Handle weapon swap
        if (Input.mouseScrollDelta.y > 0f)
        {
            EquipWeapon(mCrrtWeaponIndex + 1);
        }
        if (Input.mouseScrollDelta.y < 0f)
        {
            EquipWeapon(mCrrtWeaponIndex - 1);
        }

        // Update UI
        PlayerHUD.SetAmmoDisplayed(mCrrtWeapon.AmmoLeft, mCrrtWeapon.ClipSize);
    }

    /// <summary> Animate weapon bob. </summary>
    void HandleWeaponBob()
    {
        // Update strength of weapon bob according to player velocity.
        Vector3 velocityInPlane = mCharacterVelocity;
        velocityInPlane.y = 0f;
        mCurrentWeaponBobFactor = Mathf.Lerp(mCurrentWeaponBobFactor, 0.15f + 0.85f * velocityInPlane.magnitude / CharacterSpeedRunning, WeaponBobSharpness * Time.deltaTime);
        mWeaponBobTime += Time.deltaTime * WeaponBobFrequency * mCurrentWeaponBobFactor;
        if (mWeaponBobTime > 2 * Mathf.PI)
        {
            mWeaponBobTime -= 2 * Mathf.PI;
        }

        //Calculate weapon bob.
        float currentWeaponBobAmplitude = mCurrentWeaponBobFactor * WeaponBobAmplitude;
        float hBobValue = Mathf.Sin(mWeaponBobTime) * currentWeaponBobAmplitude;
        float vBobValue = (1f - Mathf.Cos(mWeaponBobTime * 2f)) * currentWeaponBobAmplitude;
        mCrrtWeapon.gameObject.transform.localPosition = new Vector3(hBobValue, vBobValue, mCrrtWeapon.gameObject.transform.localPosition.z);
    }

    /// <summary> Receive ability input and respond to it. </summary>
    void HandleAbilities()
    {
        // DASH
        if (Time.time < mTimeDashStart + DashDuration)
        {
            mCharacterVelocity = mDashVelocity;
        }
        else if (Input.GetButtonDown("Dash"))
        {
            mTimeDashStart = Time.time;
            PlayerHUD.ResetDashIcon();
            mCharacterVelocity.y = 0f;
            mCharacterVelocity.Normalize();
            mDashVelocity = DashSpeed * mCharacterVelocity;
        }
    }

    // Used by timeline signals to deactivate movement during cutscenes
    public void ToggleDisableMovement()
    {
        mDisableMovement = !mDisableMovement;
    }

    // Used by timeline signals to deactivate weapons during cutscenes
    public void ToggleDisableWeapon()
    {
        mDisableWeapons = !mDisableWeapons;
    }
}