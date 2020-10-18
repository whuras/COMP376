using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform CameraRoot;
    public float CharacterSpeedNormal;
    public float CharacterSpeedRunning;
    public float JumpVelocity;
    public float GravityAcceleration;
    public Vector2 MouseSensitivity;
    public Weapon weapon;

    CharacterController mCharacterController;
    Vector3 mCharacterVelocity = new Vector3(0f,0f,0f);

    void Start()
    {
        mCharacterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        }
        HandleMovement();
        HandleWeapons();    
    }

    void HandleMovement ()
    {
        // WALKING/RUNNING
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        float speed = Mathf.Lerp(CharacterSpeedNormal, CharacterSpeedRunning, Input.GetAxis("Run"));
        moveDirection.Normalize();
        moveDirection = transform.TransformDirection(moveDirection);

        // JUMPING
        float verticalVelocity = mCharacterVelocity.y;
        if (mCharacterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = JumpVelocity;
            }
        }
        else
        {
            verticalVelocity -= GravityAcceleration * Time.deltaTime;
        }
        mCharacterVelocity = moveDirection * speed + Vector3.up * verticalVelocity;
        mCharacterController.Move(mCharacterVelocity * Time.deltaTime);

        // AIMING (LEFT/RIGHT)
        Vector3 characterRotation = transform.localEulerAngles;
        float angleDelta =  Input.GetAxis("Mouse X") * MouseSensitivity.x * Time.deltaTime;
        characterRotation.y += angleDelta;
        transform.localEulerAngles = characterRotation;

        // AIMING (UP/DOWN)
        Vector3 cameraRotation = CameraRoot.transform.localEulerAngles;
        angleDelta =  Input.GetAxis("Mouse Y") * MouseSensitivity.y * Time.deltaTime;
        cameraRotation.x = cameraRotation.x - angleDelta;
        CameraRoot.transform.localEulerAngles = cameraRotation;
    }

    void HandleWeapons ()
    {
        weapon.ReceiveFireInputs(
            Input.GetButtonDown("Fire1"),
            Input.GetButton("Fire1"),
            Input.GetButtonUp("Fire1"));
    }
}
