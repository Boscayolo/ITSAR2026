using UnityEngine;

/// <summary>
/// Movimento del giocatore con CharacterController e nuovo Input System.
/// Il movimento è relativo alla camera: "su" significa sempre "verso l'alto dello schermo".
/// Supporta effetti temporanei (scivolamento, rallentamento, accelerazione) applicati dall'esterno.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField, Tooltip("Gradi al secondo")] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -20f;

    [Header("Riferimenti")]
    [SerializeField, Tooltip("Se vuoto usa la Main Camera")] private Transform cameraTransform;

    private CharacterController controller;
    private GameControls controls;
    private float verticalVelocity;
    private Vector3 horizontalVelocity;

    // Effetti temporanei: ognuno ha il proprio timer, riapplicarli rinnova la durata (non si sommano).
    private float slideTimer;
    private float slideAcceleration;
    private float slowTimer;
    private float slowMultiplier = 1f;
    private float boostTimer;
    private float boostMultiplier = 1f;

    public bool IsSliding => slideTimer > 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new GameControls();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Le action vanno abilitate e disabilitate insieme al componente.
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();

    // ---- API effetti temporanei ----

    /// <summary>Scivolamento: l'accelerazione orizzontale diventa bassa, quindi si accumula inerzia.</summary>
    public void ApplySlide(float duration, float acceleration)
    {
        slideTimer = duration;
        slideAcceleration = acceleration;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        slowTimer = duration;
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        boostMultiplier = multiplier;
        boostTimer = duration;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        slideTimer -= dt;
        slowTimer -= dt;
        boostTimer -= dt;

        Vector2 input = controls.Player.Move.ReadValue<Vector2>();
        Vector3 direction = CameraRelative(input);

        float speed = moveSpeed;
        if (controls.Player.Sprint.IsPressed())
        {
            speed *= sprintMultiplier;
        }
        if (slowTimer > 0f) speed *= slowMultiplier;
        if (boostTimer > 0f) speed *= boostMultiplier;

        // Velocità orizzontale: istantanea di norma, graduale (inerzia) durante lo scivolamento.
        Vector3 targetVelocity = direction * speed;
        if (slideTimer > 0f)
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, slideAcceleration * dt);
        }
        else
        {
            horizontalVelocity = targetVelocity;
        }

        // Gravità applicata a mano: il CharacterController non usa la fisica.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * dt;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * dt);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * dt);
        }
    }

    /// <summary>Converte l'input 2D in una direzione sul piano XZ, orientata come la camera.</summary>
    private Vector3 CameraRelative(Vector2 input)
    {
        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}