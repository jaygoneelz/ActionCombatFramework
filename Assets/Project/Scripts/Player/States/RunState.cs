using ActionCombat.Core.StateMachine;

namespace ActionCombat.Player.States
{
    public class RunState : PlayerState
    {
        public RunState(StateMachine stateMachine, PlayerController player)
            : base("Run", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            animator.PlayAnimation("Run");
        }

        public override void Execute()
        {
            base.Execute();
            movement.Move(input.MoveInput);
            animator.SetFloat("Speed", movement.NormalisedSpeed);
        }
    }
}