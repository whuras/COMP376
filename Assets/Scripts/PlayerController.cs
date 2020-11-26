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
    [Tooltip("Distance traveled per foot step")]
    public float StrideLength;
    [Tooltip("Foot step sound effects for walking on wood")]
    public AudioClip[] WoodFootStepSoundEffects;

    [Header("Abilities")]
    [Tooltip("Speed of Dash ability")]
    public float DashSpeed = 20f;
    [Tooltip("Duration of dash ability")]
    public float DashDuration = 0.25f;
    [Tooltip("Factor by which time scale is multiplied during slowdown")]
    public float SlowdownFactor = 0.5f;

    //Other game objects
    Conductor mConductor;

    // Components
    CharacterController mCharacterController;
    AudioSource mAudioSource;

    // Movement
    Vector3 mCharacterVelocity = new Vector3(0f,0f,0f);
    float mTimeLastJump = -10f;
    bool mIsGrounded;
    
    // Animation
    float mTimeLastStep = 0f;
    float mCurrentWeaponBobFactor;
    float mWeaponBobTime = 0f;

    // Weaponry
    Weapon mCrrtWeapon;
    int mCrrtWeaponIndex = 0;

    // Abilities
    float mTimeLastForward = -10f;
    float mTimeLastLeft = -10f;
    float mTimeLastBackward = -10f;
    float mTimeLastRight = -10f;
    float mTimeSecondDashPress = -10f;
    bool isDashing = false;
    
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
        HandleMovement();
        HandleWeapons();
        HandleWeaponBob();
        HandleAbilities();
    }

    /// <summary> Delay physics updates to syncronize with physics system. </summary>
    void FixedUpdate()
    {
        if (!isDashing)
        {
            mCharacterController.Move(mCharacterVelocity * Time.deltaTime);
        }
    }

    /// <summary> Update whether or not player is grounded. </summary>
    void CheckIfGrounded()
    {
        mIsGrounded =  mTimeLastJump + 0.2f < Time.time
                    && Physics.Raycast(transform.position, Vector3.down, 1.6f);
    }

    /// <summary> Play footstep sound effect depending on ground material and player speed. </summary>
    void PlayFootStep(float speed)
    {
        if (Time.time - mTimeLastStep > StrideLength / speed && mIsGrounded && Time.time > mTimeSecondDashPress + DashDuration)
        {
            mAudioSource.PlayOneShot(WoodFootStepSoundEffects[Random.Range(0, WoodFootStepSoundEffects.Length-1)], Random.Range(0.5f, 1.0f));
            mTimeLastStep = Time.time;
        }
    }

    /// <summary> Receive player movement input and respond to it. </summary>
    void HandleMovement ()
    {
        // WALKING/RUNNING
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
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
    void HandleWeapons ()
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
            EquipWeapon(mCrrtWeaponIndex+1);
        }
        if (Input.mouseScrollDelta.y < 0f)
        {
            EquipWeapon(mCrrtWeaponIndex-1);
        }

        // Update UI
        PlayerHUD.SetAmmoDisplayed(mCrrtWeapon.AmmoLeft, mCrrtWeapon.ClipSize);
    }

    /// <summary> Animate weapon bob. </summary>
    void HandleWeaponBob()
    {
        // Update strength of weapon bob according to player velocity.
        mCurrentWeaponBobFactor = Mathf.Lerp(mCurrentWeaponBobFactor, 0.15f + 0.85f*mCharacterVelocity.magnitude/CharacterSpeedRunning, WeaponBobSharpness * Time.deltaTime);
        mWeaponBobTime += Time.deltaTime * WeaponBobFrequency * mCurrentWeaponBobFactor;
        if (mWeaponBobTime > 2*Mathf.PI)
        {
            mWeaponBobTime -= 2*Mathf.PI;
        }

        //Calculate weapon bob.
        float currentWeaponBobAmplitude = mCurrentWeaponBobFactor * WeaponBobAmplitude;
        float hBobValue = Mathf.Sin(mWeaponBobTime) * currentWeaponBobAmplitude;
        float vBobValue = (1f - Mathf.Cos(mWeaponBobTime * 2f)) * currentWeaponBobAmplitude;
        mCrrtWeapon.gameObject.transform.localPosition = new Vector3(hBobValue, vBobValue, mCrrtWeapon.gameObject.transform.localPosition.z);
    }

    /// <summary> Receive ability input and respond to it. </summary>
    void HandleAbilities ()
    {
        // DASH
        if (Input.GetKeyUp(KeyCode.C))
        {
            StartCoroutine(Dash());
            isDashing = false;
        }

        //SLOW-MOTION
        if (Input.GetButtonDown("Slowdown"))
        {
            mConductor.SetSpeed(SlowdownFactor);
        }
        if (Input.GetButtonUp("Slowdown"))
        {
            mConductor.SetSpeed(1f);
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        float startTime = Time.time;

        while(Time.time < startTime + DashDuration)
        {
            mCharacterController.Move(mCharacterVelocity * DashSpeed * Time.deltaTime);

            yield return null;
        }
    }
}

