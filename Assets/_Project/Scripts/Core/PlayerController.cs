using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform cameraTransform;
    private GameControls controls;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float gravity = 10f;
    [SerializeField] float rotationSpeed = 720f;

    //In Awake viene assegnato un nuovo valore di oggetto GameControls a controls (non assegnato)
    private void Awake() => controls = new GameControls();

    //private void Awake()
    //{
    //    controls = new GameControls();
    //}

    //dopo aver assegnato un valore a GameControls ne abilitiamo/attiviamo la mappa Player
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();

    private void Update()
    {
        ApplyGravity();
        MoveCharacter();
        RotateToDirection();
    }

    void MoveCharacter()
    {
        // lettura continua: valore tra (-1,-1) e (1,1)
        Vector2 input = controls.Player.Move.ReadValue<Vector2>();
        Debug.Log(input);

        float speed = moveSpeed;
        if (controls.Player.Sprint.IsPressed())  // tenuto premuto
            speed *= sprintMultiplier;

        //Vector3 moveVector = new Vector3(input.x /*X*/, 0 /*Y*/, input.y /*Z*/) * speed;
        Vector3 moveVector = CameraRelative(input) * speed;

        characterController.Move(moveVector * Time.deltaTime);
    }

    void ApplyGravity()
    {
        float verticalVelocity = 0f;
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;          // resta appoggiato
        }
        else
        {
            verticalVelocity = -gravity;
        }

        Vector3 velocity = Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    void RotateToDirection()
    {
        // lettura continua: valore tra (-1,-1) e (1,1)
        Vector2 directionRaw = controls.Player.Move.ReadValue<Vector2>();
        Vector3 direction = CameraRelative(directionRaw);

        if (direction.sqrMagnitude > 0.001f)   // si sta muovendo
        {
            // guardare nella direzione di movimento
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // girarsi gradualmente
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }

    }

    private Vector3 CameraRelative(Vector2 input)
    {
        if (cameraTransform == null)          // nessuna camera
            return new Vector3(input.x, 0f, input.y);

        //Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;                // appiattito sul terreno
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        // ClampMagnitude: in diagonale non si va più veloci
        return Vector3.ClampMagnitude(
            forward * input.y + right * input.x, 1f);
    }


}
