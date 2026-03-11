using System.Collections.Generic;
using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// Attach to any entity that can deal damage (player, enemies).
    /// Activates a sphere-cast hitbox when told to by the state machine.
    /// Tracks what's been hit this swing to prevent double-hits.
    /// </summary>
    public class HitboxController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private HitboxData defaultHitboxData;
        [SerializeField] private LayerMask targetLayer;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        private HitboxData activeHitboxData;
        private bool isActive;
        private HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

        // Debug data for gizmo drawing
        private Vector3 lastCheckPosition;
        private float lastCheckRadius;
        private bool lastHitSomething;
        private float hitFlashTimer;

        public bool IsActive => isActive;

        /// <summary>
        /// Call when attack starts. Pass specific hitbox data
        /// or null to use the default.
        /// </summary>
        public void Activate(HitboxData data = null)
        {
            activeHitboxData = data != null ? data : defaultHitboxData;
            isActive = true;
            hitThisSwing.Clear();
        }

        /// <summary>
        /// Call when attack active frames end.
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
        }

        private void Update()
        {
            if (!isActive || activeHitboxData == null) return;

            CheckHits();

            if (hitFlashTimer > 0f)
                hitFlashTimer -= Time.deltaTime;
        }

        private void CheckHits()
        {
            // Calculate hitbox position in world space
            Vector3 worldOffset = transform.TransformPoint(activeHitboxData.offset);
            lastCheckPosition = worldOffset;
            lastCheckRadius = activeHitboxData.radius;

            Collider[] hits = Physics.OverlapSphere(worldOffset, activeHitboxData.radius, targetLayer);
            lastHitSomething = false;

            foreach (Collider hit in hits)
            {
                // Skip self
                if (hit.transform.root == transform.root) continue;

                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = hit.GetComponentInParent<IDamageable>();

                if (damageable == null) continue;
                if (!damageable.IsAlive) continue;
                if (hitThisSwing.Contains(damageable)) continue;

                // Register hit to prevent double-damage
                hitThisSwing.Add(damageable);

                // Build damage data
                Vector3 hitDirection = (hit.transform.position - transform.position).normalized;
                DamageData damageData = new DamageData(
                    baseDamage: activeHitboxData.baseDamage,
                    hitPoint: hit.ClosestPoint(worldOffset),
                    hitDirection: hitDirection,
                    source: gameObject,
                    type: activeHitboxData.damageType,
                    knockbackForce: activeHitboxData.knockbackForce,
                    staggerDuration: activeHitboxData.staggerDuration
                );

                damageable.TakeDamage(damageData);
                lastHitSomething = true;
                hitFlashTimer = 0.15f;

                UnityEngine.Debug.Log($"[Hitbox] {gameObject.name} hit {hit.gameObject.name} " +
                    $"for {activeHitboxData.baseDamage} {activeHitboxData.damageType} damage");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            if (isActive && activeHitboxData != null)
            {
                // Active hitbox: red, or yellow flash on hit
                if (hitFlashTimer > 0f)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = new Color(1f, 0f, 0f, 0.6f);

                Gizmos.DrawWireSphere(lastCheckPosition, lastCheckRadius);
                Gizmos.DrawSphere(lastCheckPosition, lastCheckRadius * 0.3f);
            }
            else if (activeHitboxData != null || defaultHitboxData != null)
            {
                // Inactive hitbox: dim outline
                HitboxData data = activeHitboxData != null ? activeHitboxData : defaultHitboxData;
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Vector3 pos = transform.TransformPoint(data.offset);
                Gizmos.DrawWireSphere(pos, data.radius);
            }
        }
    }
}