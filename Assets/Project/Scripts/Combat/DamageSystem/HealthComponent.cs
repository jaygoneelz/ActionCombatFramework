using System;
using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// Manages health for any entity. Attach to player, enemies,
    /// destructibles. Fires events for UI, audio, VFX to react.
    /// </summary>
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Defence")]
        [SerializeField] private float damageReduction = 0f;
        [SerializeField] private float blockDamageReduction = 0.8f;

        // Events for UI, audio, VFX
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<DamageData> OnDamaged;
        public event Action OnDeath;

        // External systems can set this (e.g., BlockState sets it true)
        public bool IsBlocking { get; set; }

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float NormalisedHealth => currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0f;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(DamageData damage)
        {
            if (!IsAlive) return;

            // Calculate final damage
            float finalDamage = damage.BaseDamage;

            // Apply block reduction
            if (IsBlocking)
            {
                finalDamage *= (1f - blockDamageReduction);
            }

            // Apply passive damage reduction
            finalDamage *= (1f - damageReduction);

            // Ensure minimum 1 damage if any damage was dealt
            if (damage.BaseDamage > 0f && finalDamage < 1f)
                finalDamage = 1f;

            currentHealth = Mathf.Max(0f, currentHealth - finalDamage);

            UnityEngine.Debug.Log($"[Health] {gameObject.name} took {finalDamage:F1} damage. " +
                $"HP: {currentHealth:F0}/{maxHealth} | Blocked: {IsBlocking}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(damage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the entity. Clamps to max health.
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Reset to full health. Use for respawning.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            UnityEngine.Debug.Log($"[Health] {gameObject.name} DIED");
            OnDeath?.Invoke();
        }

        public Transform GetTransform() => transform;
    }
}