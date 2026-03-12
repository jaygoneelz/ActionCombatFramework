using ActionCombat.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ActionCombat.UI
{
    /// <summary>
    /// Floating health bar that follows a target. Uses world-space Canvas.
    /// Attach to a Canvas that is a child of the entity.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image delayedFillImage;

        [Header("Settings")]
        [SerializeField] private float heightOffset = 2.3f;
        [SerializeField] private float delayedFillSpeed = 2f;
        [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private float lowHealthThreshold = 0.3f;

        private float delayedFillAmount = 1f;
        private Transform followTarget;

        private void Start()
        {
            if (healthComponent == null)
                healthComponent = GetComponentInParent<HealthComponent>();

            if (healthComponent != null)
            {
                followTarget = healthComponent.transform;
                healthComponent.OnHealthChanged += OnHealthChanged;

                // Initialise
                UpdateFill(healthComponent.NormalisedHealth);
                delayedFillAmount = healthComponent.NormalisedHealth;
            }
        }

        private void Update()
        {
            // Follow target
            if (followTarget != null)
            {
                transform.position = followTarget.position + Vector3.up * heightOffset;
            }

            // Always face camera
            if (Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }

            // Smooth delayed fill (the "damage ghost" bar)
            if (delayedFillImage != null)
            {
                float currentFill = fillImage != null ? fillImage.fillAmount : 0f;
                delayedFillAmount = Mathf.MoveTowards(
                    delayedFillAmount, currentFill, delayedFillSpeed * Time.deltaTime);
                delayedFillImage.fillAmount = delayedFillAmount;
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            float normalised = current / max;
            UpdateFill(normalised);
        }

        private void UpdateFill(float normalised)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = normalised;

                // Lerp color based on health
                fillImage.color = normalised > lowHealthThreshold
                    ? fullHealthColor
                    : Color.Lerp(lowHealthColor, fullHealthColor, normalised / lowHealthThreshold);
            }
        }

        private void OnDestroy()
        {
            if (healthComponent != null)
                healthComponent.OnHealthChanged -= OnHealthChanged;
        }
    }
}