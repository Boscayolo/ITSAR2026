using UnityEngine;

public class PlayerController_Rigidbody : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 input;
    private GameControls controls;
    [SerializeField] private float force = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new GameControls();
    }

    // Le action vanno abilitate e disabilitate insieme al componente.
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();

    private void Update()// ogni frame: leggo
    {
        input = controls.Player.Move.ReadValue<Vector2>();
        Debug.Log(input);
    }

    private void FixedUpdate()
    {
        // dinamico: una spinta
        Vector3 input3 = new Vector3(input.x, 0f, input.y);
        Vector3 velocity = input3 * force;

        //rb.AddForce(velocity); //AddForce con rigidbody non kinematic

        rb.MovePosition(rb.position + 
            velocity * Time.fixedDeltaTime); //MovePosition con rigidbody kinematic
    }




}
