using UnityEngine;

/// <summary>
/// Fa pulsare l'emissione del materiale (URP Lit) con un'onda sinusoidale.
/// L'intensità oscilla tra minIntensity e maxIntensity.
/// </summary>
public class PulseLight : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("Target")]
    [Tooltip("Renderer del cristallo. Se vuoto viene cercato in questo oggetto o nei figli.")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Colore")]
    [Tooltip("Colore base dell'emissione (l'intensità viene applicata come moltiplicatore).")]
    [SerializeField] private Color emissionColor = new Color(0.2f, 0.9f, 1f);

    [Header("Pulsazione")]
    [Min(0f)][SerializeField] private float minIntensity = 0.5f;
    [Min(0f)][SerializeField] private float maxIntensity = 3f;
    [Tooltip("Pulsazioni complete al secondo (Hz).")]
    [Min(0f)][SerializeField] private float pulseSpeed = 0.5f;

    private Material materialInstance;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("PulseLight: nessun Renderer trovato.", this);
            enabled = false;
            return;
        }

        // .material crea un'istanza: il materiale condiviso nel progetto non viene modificato.
        materialInstance = targetRenderer.material;
        materialInstance.EnableKeyword("_EMISSION");
        materialInstance.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
    }

    private void Update()
    {
        // Sin va da -1 a 1: lo rimappiamo a 0..1 e poi a min..max.
        float t = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        materialInstance.SetColor(EmissionColorId, emissionColor.linear * intensity);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }

    private void OnValidate()
    {
        if (maxIntensity < minIntensity)
            maxIntensity = minIntensity;
    }
}