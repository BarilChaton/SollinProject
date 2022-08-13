using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool CanMove { get; set; } = true;
    private bool isSprinting => canSprint && Input.GetKey(sprinteKey);
    private bool shouldJump => Input.GetKey(jumpKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool isRunningToggled = false;

    [Header("Controls")]
    [SerializeField] private KeyCode sprinteKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode runToggleKey = KeyCode.CapsLock;
    [SerializeField] private KeyCode mouseToggleKey = KeyCode.LeftAlt;
    [SerializeField] private KeyCode SeatheWeapon = KeyCode.R;

    [Header("Movement Parameters")]
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float sprintSpeed = 12.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);

    [Header("Weapon Stuffs")]
    public GameObject weaponHolder;
    public Component weaponController;

    private bool isCrouching;
    private bool duringCrouchAnimation;
    private bool mouseCursorIsToggled = false;
    private bool weaponIsUp = false;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        weaponController = GetComponentInChildren<WeaponControl>();
        weaponHolder.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();

            if(!mouseCursorIsToggled)
                HandleMouseLook();

            if (canJump)
                HandleJump();

            if (canCrouch)
                HandleCrouch();

            HandleMouseToggle();
            HandleWeapon();

            ApplyFinalMovements();
        }

        if (Input.GetKeyUp(runToggleKey) && !isRunningToggled)
        {
            isRunningToggled = true;
        }
        else if (Input.GetKeyUp(runToggleKey))
        {
            isRunningToggled = false;
        }
    }

    private void HandleMovementInput()
    {
        float moveDirectionY = moveDirection.y;

        if (isRunningToggled)
        {
            currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : runSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : runSpeed) * Input.GetAxis("Horizontal"));

            moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
            moveDirection.y = moveDirectionY;
        }
        else
        {
            currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

            moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
            moveDirection.y = moveDirectionY;
        }
        
    }

    private void HandleMouseToggle()
    {
        if (Input.GetKeyUp(mouseToggleKey) && !mouseCursorIsToggled)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            mouseCursorIsToggled = true;
        }
        else if (Input.GetKeyUp(mouseToggleKey) && mouseCursorIsToggled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mouseCursorIsToggled = false;
        }
    }

    private void HandleJump()
    {
        if (shouldJump)
            moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
        if (shouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleWeapon()
    {
        if (Input.GetKeyUp(SeatheWeapon) && !weaponIsUp)
        {
            weaponHolder.SetActive(true);
            weaponIsUp = true;
        }
        else if (Input.GetKeyUp(SeatheWeapon) && weaponIsUp) 
        {
            weaponHolder.SetActive(false);
            weaponIsUp = false;
        }
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
