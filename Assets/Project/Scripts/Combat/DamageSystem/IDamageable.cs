using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// Any GameObject that can receive damage implements this.
    /// Player, enemies, destructibles — all use the same interface.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageData damage);
        bool IsAlive { get; }
        Transform GetTransform();
    }
}