using UnityEngine;

// Controlador de primera persona para PROTOTIPAR EN ESCRITORIO.
// Cuando migres a Meta Quest, reemplaza este script por un XR Origin
// (XR Interaction Toolkit) con locomoción por joystick/teleport.
// El resto de los scripts (EventLogger, Act1Manager, etc.) NO dependen
// de este controlador, así que la migración a VR no rompe nada más.

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 3.5f;
    public float lookSensitivity = 2f;
    public Camera playerCamera;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;
    private const float gravity = -9.81f;

    public Animator animator; // Opcional: para animar al jugador (ej. caminar)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMove()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + gravity * Time.deltaTime;

        Vector3 velocity = move * walkSpeed;
        velocity.y = verticalVelocity;

        animator.SetFloat("Speed", move.magnitude); // Opcional: para animar al jugador (ej. caminar)

        controller.Move(velocity * Time.deltaTime);
    }
}