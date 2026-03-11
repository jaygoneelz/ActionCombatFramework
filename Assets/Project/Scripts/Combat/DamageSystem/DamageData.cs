using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// Immutable data packet describing a single hit. Created by hitboxes,
    /// consumed by IDamageable. Carries everything needed for damage
    /// calculation, knockback, and VFX.
    /// </summary>
    [System.Serializable]
    public struct DamageData
    {
        public float BaseDamage;
        public float KnockbackForce;
        public Vector3 HitPoint;
        public Vector3 HitDirection;
        public GameObject Source;
        public DamageType Type;
        public float StaggerDuration;

        public DamageData(float baseDamage, Vector3 hitPoint, Vector3 hitDirection,
            GameObject source, DamageType type = DamageType.Light,
            float knockbackForce = 2f, float staggerDuration = 0.3f)
        {
            BaseDamage = baseDamage;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Source = source;
            Type = type;
            KnockbackForce = knockbackForce;
            StaggerDuration = staggerDuration;
        }
    }

    public enum DamageType
    {
        Light,
        Heavy,
        Special
    }
}