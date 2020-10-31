using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform CameraRoot;
    public float CharacterSpeedNormal;
    public float CharacterSpeedRunning;
    public float JumpVelocity;
    public float GravityAcceleration;
    public Vector2 MouseSensitivity;

    public Weapon Weapon;
    public float WeaponBobAmplitude;
    public float WeaponBobFrequency;
    public float WeaponBobSharpness;

    public PlayerInterface playerInterface;
    public HealthController healthController;

    CharacterController mCharacterController;
    Vector3 mCharacterVelocity = new Vector3(0f,0f,0f);
    float mCurrentWeaponBobFactor;
    float mWeaponBobTime;
    bool mIsGrounded;
    float mTimeLastJump = 0f;

    void Start()
    {
        // Initial Player Health
        healthController = gameObject.GetComponent<HealthController>();
        healthController.OnDamaged += HealthController_OnDamaged;
        healthController.OnHealed += HealthController_OnHealed;

        // Initial User Interface
        playerInterface.SetHealthBar(healthController.GetHealthNormalized());
        playerInterface.SetAmmoCount(weapon);
        
        // Initialize Controls
        mCharacterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        mWeaponBobTime = Time.time;
    }

    void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        }
        CheckIfGrounded();
        HandleMovement();
        HandleWeapons();
        HandleWeaponBob();
    }

    void CheckIfGrounded()
    {
        mIsGrounded = mTimeLastJump + 0.2f < Time.time && Physics.Raycast(transform.position, Vector3.down, 1.6f);
    }

    void HandleMovement()
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
                verticalVelocity = JumpVelocity;
            }
        }
        else
        {
            verticalVelocity = mCharacterVelocity.y - GravityAcceleration * Time.deltaTime;
        }
        mCharacterVelocity = moveDirection * speed + Vector3.up * verticalVelocity;
        mCharacterController.Move(mCharacterVelocity * Time.deltaTime);

        // AIMING (LEFT/RIGHT)
        Vector3 characterRotation = transform.localEulerAngles;
        float angleDelta = Input.GetAxis("Mouse X") * MouseSensitivity.x * Time.deltaTime;
        characterRotation.y += angleDelta;
        transform.localEulerAngles = characterRotation;

        // AIMING (UP/DOWN)
        Vector3 cameraRotation = CameraRoot.transform.localEulerAngles;
        angleDelta = Input.GetAxis("Mouse Y") * MouseSensitivity.y * Time.deltaTime;
        cameraRotation.x = cameraRotation.x - angleDelta;
        CameraRoot.transform.localEulerAngles = cameraRotation;
    }

    void HandleWeapons()
    {
        Weapon.ReceiveFireInputs(
            Input.GetButtonDown("Fire1"),
            Input.GetButton("Fire1"),
            Input.GetButtonUp("Fire1"));
        playerInterface.UpdateAmmoCount(weapon);
    }

    void HealthController_OnHealed()
    {
        playerInterface.UpdateHealthBar(true, healthController.GetHealthNormalized());
    }

    void HealthController_OnDamaged()
    {
        playerInterface.UpdateHealthBar(false, healthController.GetHealthNormalized());
    }

    void HandleWeaponBob()
    {
        // Update strength of weapon bob according to player velocity.
        mCurrentWeaponBobFactor = Mathf.Lerp(mCurrentWeaponBobFactor, 0.85f*mCharacterVelocity.magnitude/CharacterSpeedRunning+0.15f, WeaponBobSharpness * Time.deltaTime);
        mWeaponBobTime += Time.deltaTime * WeaponBobFrequency * mCurrentWeaponBobFactor;
        if (mWeaponBobTime > 2*Mathf.PI)
        {
            mWeaponBobTime -= 2*Mathf.PI;
        }

        //Calculate weapon bob.
        float currentWeaponBobAmplitude = mCurrentWeaponBobFactor * WeaponBobAmplitude;
        float hBobValue = Mathf.Sin(mWeaponBobTime) * currentWeaponBobAmplitude * 0.5f;
        float vBobValue = 0.5f * (1f - Mathf.Cos(mWeaponBobTime * 2f)) * currentWeaponBobAmplitude;
        Weapon.gameObject.transform.localPosition = new Vector3(hBobValue, vBobValue, Weapon.gameObject.transform.localPosition.z);
    }
}

