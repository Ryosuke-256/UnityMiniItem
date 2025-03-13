using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float sensitivity = 2.0f;
    [SerializeField] private float maxYAngle = 80f;
    [SerializeField] private float walkingSpeed = 5.0f;
    [SerializeField] private float flyingSpeed = 10.0f;
    [SerializeField] float mass = 1.0f;
    [SerializeField] float acceleration =20.0f;
    [SerializeField] float jumpSpeed = 5.0f;

    public State state;
    public enum State
    {
        Walking,
        Flying
    }

    CharacterController controller;
    Vector3 velocity;
    private Vector2 look;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction flyUpDownAction;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
        lookAction = playerInput.actions["look"];
        jumpAction = playerInput.actions["jump"];
        flyUpDownAction = playerInput.actions["flyUpDown"];
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        switch (state)
        {
            case State.Walking:
                UpdateGravity();
                UpdateLook();
                UpdateMovement();
                break;
            case State.Flying:
                UpdateLook();
                UpdateMovementFlying();
                break;
        }
    }

    void UpdateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
    }

    Vector3 GetMovementInput(float speed,bool horizontal = true)
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var flyUpDownInput = flyUpDownAction.ReadValue<float>();
        var input = new Vector3();
        var referenceTransform = horizontal ? transform : cameraTransform;
        input += referenceTransform.forward * moveInput.y;
        input += referenceTransform.right * moveInput.x;
        input = Vector3.ClampMagnitude(input, 1f);
        if (!horizontal)
        {
            input += transform.up * flyUpDownInput;
        }
        input *= speed;

        return input;
    }

    void UpdateMovement()
    {
        var input = GetMovementInput(walkingSpeed);

        var factor = acceleration * Time.deltaTime;
        velocity.x = Mathf.Lerp(velocity.x, input.x, factor);
        velocity.z = Mathf.Lerp(velocity.z, input.z, factor);

        var jumpInput = jumpAction.ReadValue<float>();
        if (jumpInput > 0 && controller.isGrounded)
        {
            velocity.y += jumpSpeed;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateMovementFlying()
    {
        var input = GetMovementInput(flyingSpeed,false);

        var factor = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, input, factor);

        var jumpInput = jumpAction.ReadValue<float>(); 
        if (jumpInput > 0 && controller.isGrounded)
        {
            velocity.y += jumpSpeed;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateLook()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        look.x += lookInput.x * sensitivity;
        look.y -= lookInput.y * sensitivity;


        look.y = Mathf.Clamp(look.y, -maxYAngle, maxYAngle);

        cameraTransform.localRotation = Quaternion.Euler(look.y, 0, 0);
        transform.rotation = Quaternion.Euler(0, look.x, 0);
    }

    void OnToggleFlying()
    {
        state = state == State.Flying ? State.Walking : State.Flying;
    }
}