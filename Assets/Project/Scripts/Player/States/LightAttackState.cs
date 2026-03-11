using ActionCombat.Core.StateMachine;
using ActionCombat.Combat;
using UnityEngine;

namespace ActionCombat.Player.States
{
    public class LightAttackState : PlayerState
    {
        private int comboStep;
        private bool comboWindowOpen;
        private bool comboQueued;
        private float stepTimer;
        private float currentStepDuration;
        private bool hitboxActivated;

        private readonly float[] stepDurations = { 0.4f, 0.35f, 0.5f };
        private readonly float[] stepDamage = { 10f, 12f, 18f };
        private readonly int maxComboSteps = 3;

        private HitboxController hitbox;

        public bool IsComplete { get; private set; }
        public int CurrentComboStep => comboStep;

        public LightAttackState(StateMachine stateMachine, PlayerController player)
            : base("LightAttack", stateMachine, player)
        {
            hitbox = player.GetComponentInChildren<HitboxController>();
        }

        public override void Enter()
        {
            base.Enter();
            comboStep = 0;
            IsComplete = false;
            StartAttackStep();
        }

        public override void Execute()
        {
            base.Execute();

            stepTimer += Time.deltaTime;
            float normalisedTime = stepTimer / currentStepDuration;

            // Hitbox active frames: 20% to 45% of step
            if (normalisedTime >= 0.2f && normalisedTime < 0.45f)
            {
                if (!hitboxActivated)
                {
                    hitboxActivated = true;
                    if (hitbox != null) hitbox.Activate();
                }
            }
            else if (normalisedTime >= 0.45f && hitboxActivated)
            {
                hitboxActivated = false;
                if (hitbox != null) hitbox.Deactivate();
            }

            // Combo window: 50% to 85%
            if (normalisedTime >= 0.5f && normalisedTime < 0.85f)
            {
                if (!comboWindowOpen)
                {
                    comboWindowOpen = true;
                }

                if (input.ConsumeLightAttack() || input.ConsumeHeavyAttack())
                {
                    comboQueued = true;
                }
            }
            else if (normalisedTime >= 0.85f)
            {
                comboWindowOpen = false;
            }

            // Step complete
            if (normalisedTime >= 1f)
            {
                // Make sure hitbox is off
                if (hitbox != null) hitbox.Deactivate();
                hitboxActivated = false;

                if (comboQueued && comboStep < maxComboSteps - 1)
                {
                    comboStep++;
                    StartAttackStep();
                }
                else
                {
                    IsComplete = true;
                }
            }

            // Forward lunge
            if (normalisedTime < 0.4f)
            {
                Vector3 forward = player.transform.forward * 2f * Time.deltaTime;
                movement.ApplyRootMotion(forward);
            }
        }

        private void StartAttackStep()
        {
            stepTimer = 0f;
            comboQueued = false;
            comboWindowOpen = false;
            hitboxActivated = false;
            currentStepDuration = stepDurations[comboStep];
            animator.PlayAnimation($"LightAttack{comboStep + 1}", 0.05f);

            UnityEngine.Debug.Log($"[Combat] Light Attack Step {comboStep + 1}/{maxComboSteps} " +
                $"| Damage: {stepDamage[comboStep]}");
        }

        public override void Exit()
        {
            base.Exit();
            if (hitbox != null) hitbox.Deactivate();
            comboStep = 0;
            comboQueued = false;
            comboWindowOpen = false;
            hitboxActivated = false;
        }
    }
}