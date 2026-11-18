using System;
using UnityEngine;

/// <summary>
/// Area con collider trigger che applica al giocatore uno o più effetti, scelti con un enum a flag.
/// Danno e cura agiscono a intervalli regolari mentre il giocatore è dentro.
/// Scivolamento, rallentamento e accelerazione restano attivi per una durata fissa dopo l'uscita.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AreaEffect : MonoBehaviour
{
    [Flags]
    public enum Effects
    {
        None = 0,
        Damage = 1 << 0,
        Heal = 1 << 1,
        Slide = 1 << 2,
        Slow = 1 << 3,
        SpeedBoost = 1 << 4,
    }

    // Raggruppamenti per gestire gli effetti in modo centralizzato.
    private const Effects TickEffects = Effects.Damage | Effects.Heal;
    private const Effects MovementEffects = Effects.Slide | Effects.Slow | Effects.SpeedBoost;

    [Header("Effetti attivi")]
    [SerializeField] private Effects effects = Effects.Damage;

    [Header("Rilevamento")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Danno / Cura (a tick)")]
    [SerializeField, Min(0f)] private float amountPerTick = 10f;
    [SerializeField, Min(0.05f), Tooltip("Secondi tra un tick e il successivo")] private float tickInterval = 1f;

    [Header("Effetti di movimento")]
    [SerializeField, Min(0f), Tooltip("Secondi per cui gli effetti restano attivi dopo l'uscita dall'area")]
    private float movementEffectDuration = 5f;
    [SerializeField, Min(0.1f), Tooltip("Slide: più è basso, più si scivola")]
    private float slideAcceleration = 3f;
    [SerializeField, Range(0.05f, 1f), Tooltip("Slow: moltiplicatore di velocità")]
    private float slowMultiplier = 0.5f;
    [SerializeField, Min(1f), Tooltip("SpeedBoost: moltiplicatore di velocità")]
    private float boostMultiplier = 1.5f;

    private Health playerHealth;
    private PlayerController playerController;
    private float tickTimer;

    private bool Has(Effects flag) => (effects & flag) != 0;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnValidate()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (Has(Effects.Damage) && Has(Effects.Heal))
        {
            Debug.LogWarning($"{name}: Damage e Heal sono entrambi attivi, si annullano a vicenda.", this);
        }
        if (Has(Effects.Slow) && Has(Effects.SpeedBoost))
        {
            Debug.LogWarning($"{name}: Slow e SpeedBoost sono entrambi attivi, i moltiplicatori si annullano a vicenda.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        playerHealth = other.GetComponentInParent<Health>();
        playerController = other.GetComponentInParent<PlayerController>();
        tickTimer = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        // Gli effetti di movimento sono già stati rinfrescati nell'ultimo Update:
        // proseguono da soli per movementEffectDuration secondi.
        ClearPlayer();
    }

    private void OnDisable()
    {
        ClearPlayer();
    }

    private void Update()
    {
        if (effects == Effects.None) return;

        // Il player può essere stato disattivato/distrutto (es. Health.Die disattiva il GameObject).
        if (playerHealth == null || playerHealth.IsDead)
        {
            ClearPlayer();
            return;
        }

        if ((effects & MovementEffects) != 0)
        {
            RefreshMovementEffects();
        }

        if ((effects & TickEffects) != 0)
        {
            tickTimer += Time.deltaTime;
            while (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;
                ApplyTick();
            }
        }
    }

    private void ApplyTick()
    {
        if (Has(Effects.Damage)) playerHealth.TakeDamage(amountPerTick);

        // Se il danno ha ucciso il player, la cura non va applicata.
        if (Has(Effects.Heal) && !playerHealth.IsDead) playerHealth.Heal(amountPerTick);
    }

    private void RefreshMovementEffects()
    {
        if (playerController == null) return;

        if (Has(Effects.Slide)) playerController.ApplySlide(movementEffectDuration, slideAcceleration);
        if (Has(Effects.Slow)) playerController.ApplySlow(slowMultiplier, movementEffectDuration);
        if (Has(Effects.SpeedBoost)) playerController.ApplySpeedBoost(boostMultiplier, movementEffectDuration);
    }

    private bool IsPlayer(Collider other)
    {
        return (playerLayer.value & (1 << other.gameObject.layer)) != 0;
    }

    private void ClearPlayer()
    {
        playerHealth = null;
        playerController = null;
        tickTimer = 0f;
    }
}