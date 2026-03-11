using ActionCombat.Core.StateMachine;
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

        private readonly float[] stepDurations = { 0.4f, 0.35f, 0.5f };
        private readonly int maxComboSteps = 3;

        public bool IsComplete { get; private set; }
        public int CurrentComboStep => comboStep;

        public LightAttackState(StateMachine stateMachine, PlayerController player)
            : base("LightAttack", stateMachine, player) { }

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

            // Combo window: 50% to 85% of current step
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

            // Forward lunge during first 40% of step
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
            currentStepDuration = stepDurations[comboStep];
            animator.PlayAnimation($"LightAttack{comboStep + 1}", 0.05f);

            UnityEngine.Debug.Log($"[Combat] Light Attack Step {comboStep + 1}/{maxComboSteps}");
        }

        public override void Exit()
        {
            base.Exit();
            comboStep = 0;
            comboQueued = false;
            comboWindowOpen = false;
        }
    }
}