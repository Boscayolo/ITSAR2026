using UnityEngine;

/// <summary>
/// Muove l'oggetto su e giù con un'onda sinusoidale tra due altezze (in coordinate locali).
/// </summary>
public class Bobbing : MonoBehaviour
{
    [Header("Altezza (Y locale)")]
    [SerializeField] private float minHeight = 1.5f;
    [SerializeField] private float maxHeight = 2.5f;

    [Header("Velocità")]
    [Tooltip("Oscillazioni complete al secondo (Hz).")]
    [Min(0f)][SerializeField] private float speed = 0.5f;

    private void Update()
    {
        // Sin va da -1 a 1: lo rimappiamo a 0..1 e poi a min..max.
        float t = Mathf.Sin(Time.time * speed * Mathf.PI * 2f) * 0.5f + 0.5f;

        Vector3 pos = transform.localPosition;
        pos.y = Mathf.Lerp(minHeight, maxHeight, t);
        transform.localPosition = pos;
    }

    // Chiamato quando aggiungi il componente (o dal menu "Reset"):
    // imposta min e max attorno all'altezza attuale dell'oggetto.
    private void Reset()
    {
        float y = transform.localPosition.y;
        minHeight = y - 0.5f;
        maxHeight = y + 0.5f;
    }

    private void OnValidate()
    {
        if (maxHeight < minHeight)
            maxHeight = minHeight;
    }
}