using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// ScriptableObject that defines hitbox properties.
    /// Create different ones for light attacks, heavy attacks, etc.
    /// Assets > Create > ActionCombat > Combat > Hitbox Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewHitboxData", menuName = "ActionCombat/Combat/Hitbox Data")]
    public class HitboxData : ScriptableObject
    {
        [Header("Damage")]
        public float baseDamage = 10f;
        public DamageType damageType = DamageType.Light;

        [Header("Knockback")]
        public float knockbackForce = 3f;
        public float staggerDuration = 0.3f;

        [Header("Hitbox Shape")]
        public float radius = 0.5f;
        public Vector3 offset = new Vector3(0f, 1f, 0.8f);
    }
}