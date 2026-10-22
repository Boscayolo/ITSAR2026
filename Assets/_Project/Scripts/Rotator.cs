using UnityEngine;

/// <summary>
/// Fa ruotare l'oggetto sull'asse Y a velocità regolabile dall'Inspector.
/// Da aggiungere all'oggetto Core.
/// </summary>
public class Rotator : MonoBehaviour
{
    [Tooltip("Velocità di rotazione in gradi al secondo. Valori negativi invertono il senso.")]
    [SerializeField] private float rotationSpeed = 45f;

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }

    /// <summary>
    /// Permette di cambiare la velocità da altri script (es. power-up, fasi di gioco).
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
}
