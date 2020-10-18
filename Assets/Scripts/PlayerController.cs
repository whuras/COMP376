using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform CameraRoot;
    public float CharacterSpeed;
    public Vector2 MouseSensitivity;
    public Weapon weapon;

    CharacterController mCharacterController;
    bool mIsGrounded = true;

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
        // JUMPING
        if (mIsGrounded && Input.GetButtonDown("Jump"))
        {
            mIsGrounded = false;
        }

        // WALKING/RUNNING
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moveDirection.Normalize();
        moveDirection = transform.TransformDirection(moveDirection);
        mCharacterController.Move(moveDirection * CharacterSpeed * Time.deltaTime);

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
