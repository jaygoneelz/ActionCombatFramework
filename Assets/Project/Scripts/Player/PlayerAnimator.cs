using System;
using UnityEngine;

namespace ActionCombat.Player
{
    /// <summary>
    /// Abstraction layer between gameplay states and Unity's Animator.
    /// States call methods here — they never touch the Animator directly.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator animator;

        public event Action OnAttackHitboxStart;
        public event Action OnAttackHitboxEnd;
        public event Action OnAttackComplete;
        public event Action OnComboWindowOpen;
        public event Action OnComboWindowClose;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
        private static readonly int TriggerAttackHash = Animator.StringToHash("TriggerAttack");
        private static readonly int TriggerDodgeHash = Animator.StringToHash("TriggerDodge");
        private static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");
        private static readonly int TriggerHitHash = Animator.StringToHash("TriggerHit");
        private static readonly int TriggerDeathHash = Animator.StringToHash("TriggerDeath");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayAnimation(string stateName, float transitionDuration = 0.1f)
        {
            animator.CrossFadeInFixedTime(stateName, transitionDuration);
        }

        public void SetFloat(string param, float value)
        {
            animator.SetFloat(Animator.StringToHash(param), value);
        }

        public void SetBool(string param, bool value)
        {
            animator.SetBool(Animator.StringToHash(param), value);
        }

        public void SetTrigger(string param)
        {
            animator.SetTrigger(Animator.StringToHash(param));
        }

        public void SetSpeed(float value) => animator.SetFloat(SpeedHash, value);
        public void SetGrounded(bool value) => animator.SetBool(IsGroundedHash, value);
        public void SetVerticalVelocity(float value) => animator.SetFloat(VerticalVelocityHash, value);

        public void TriggerAttack(int attackIndex)
        {
            animator.SetInteger(AttackIndexHash, attackIndex);
            animator.SetTrigger(TriggerAttackHash);
        }

        public void TriggerDodge() => animator.SetTrigger(TriggerDodgeHash);
        public void SetBlocking(bool value) => animator.SetBool(IsBlockingHash, value);
        public void TriggerHit() => animator.SetTrigger(TriggerHitHash);
        public void TriggerDeath() => animator.SetTrigger(TriggerDeathHash);

        public float GetNormalisedTime(int layerIndex = 0)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return info.normalizedTime % 1f;
        }

        public bool IsInTransition(int layerIndex = 0)
        {
            return animator.IsInTransition(layerIndex);
        }

        // --- Animation Event Receivers ---
        // Add these as Animation Events on your attack clips

        public void AnimEvent_HitboxStart()
        {
            OnAttackHitboxStart?.Invoke();
        }

        public void AnimEvent_HitboxEnd()
        {
            OnAttackHitboxEnd?.Invoke();
        }

        public void AnimEvent_AttackComplete()
        {
            OnAttackComplete?.Invoke();
        }

        public void AnimEvent_ComboWindowOpen()
        {
            OnComboWindowOpen?.Invoke();
        }

        public void AnimEvent_ComboWindowClose()
        {
            OnComboWindowClose?.Invoke();
        }
    }
}