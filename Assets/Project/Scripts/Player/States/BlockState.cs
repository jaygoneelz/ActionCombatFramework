using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.Player.States
{
    public class BlockState : PlayerState
    {
        /// <summary>
        /// Other systems check this to apply damage reduction.
        /// </summary>
        public bool IsActivelyBlocking { get; private set; }

        public BlockState(StateMachine stateMachine, PlayerController player)
            : base("Block", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            IsActivelyBlocking = true;
            animator.SetBlocking(true);
            animator.PlayAnimation("Block", 0.1f);

            UnityEngine.Debug.Log("[Combat] Block Start");
        }

        public override void Execute()
        {
            base.Execute();

            // Slow movement while blocking (25% speed)
            if (input.MoveInput.sqrMagnitude > 0.01f)
            {
                Vector2 reducedInput = input.MoveInput * 0.25f;
                movement.Move(reducedInput);
            }
            else
            {
                movement.Decelerate();
            }
        }

        public override void Exit()
        {
            base.Exit();
            IsActivelyBlocking = false;
            animator.SetBlocking(false);

            UnityEngine.Debug.Log("[Combat] Block End");
        }
    }
}