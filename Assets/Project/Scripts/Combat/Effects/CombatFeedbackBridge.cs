using ActionCombat.UI;
using UnityEngine;

namespace ActionCombat.Combat.Effects
{
    /// <summary>
    /// Attach to any entity (player or enemy) that should trigger
    /// visual feedback when hit. Connects HealthComponent events
    /// to HitStop, ScreenShake, and DamagePopups automatically.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class CombatFeedbackBridge : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool triggerHitStop = true;
        [SerializeField] private bool triggerScreenShake = true;
        [SerializeField] private bool triggerDamagePopup = true;
        [SerializeField] private bool triggerHitVFX = true;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;
        [SerializeField] private float vfxLifetime = 0.5f;

        private HealthComponent health;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            health.OnDamaged += OnDamaged;
        }

        private void OnDamaged(DamageData data)
        {
            if (triggerHitStop)
                HitStop.Trigger(data.Type);

            if (triggerScreenShake)
                ScreenShake.Trigger(data.Type);

            if (triggerDamagePopup)
                DamagePopupSpawner.Spawn(data.BaseDamage, data.HitPoint, data.Type);

            if (triggerHitVFX && hitVFXPrefab != null)
            {
                GameObject vfx = Instantiate(hitVFXPrefab, data.HitPoint, Quaternion.identity);
                Destroy(vfx, vfxLifetime);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
                health.OnDamaged -= OnDamaged;
        }
    }
}