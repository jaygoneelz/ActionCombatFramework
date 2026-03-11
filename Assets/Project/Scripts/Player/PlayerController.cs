using ActionCombat.Core.StateMachine;
using ActionCombat.Player.States;
using UnityEngine;
using SM = ActionCombat.Core.StateMachine.StateMachine;

namespace ActionCombat.Player
{
    /// <summary>
    /// Main player orchestrator. Creates all states, defines transitions,
    /// and pumps the state machine each frame.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerMovement Movement { get; private set; }
        public PlayerInputHandler Input { get; private set; }
        public PlayerAnimator Animator { get; private set; }

        private SM stateMachine;

        public IdleState IdleState { get; private set; }
        public RunState RunState { get; private set; }
        public JumpState JumpState { get; private set; }

        public SM DebugStateMachine => stateMachine;

        private void Awake()
        {
            Movement = GetComponent<PlayerMovement>();
            Input = GetComponent<PlayerInputHandler>();
            Animator = GetComponentInChildren<PlayerAnimator>();

            InitialiseStateMachine();
        }

        private void InitialiseStateMachine()
        {
            stateMachine = new SM();

            IdleState = new IdleState(stateMachine, this);
            RunState = new RunState(stateMachine, this);
            JumpState = new JumpState(stateMachine, this);

            // Idle transitions
            IdleState.AddTransition(new Transition(
                "Idle→Jump",
                JumpState,
                () => Input.ConsumeJump() && Movement.IsGrounded));

            IdleState.AddTransition(new Transition(
                "Idle→Run",
                RunState,
                () => Input.MoveInput.sqrMagnitude > 0.01f));

            // Run transitions
            RunState.AddTransition(new Transition(
                "Run→Jump",
                JumpState,
                () => Input.ConsumeJump() && Movement.IsGrounded));

            RunState.AddTransition(new Transition(
                "Run→Idle",
                IdleState,
                () => Input.MoveInput.sqrMagnitude < 0.01f));

            // Jump transitions
            JumpState.AddTransition(new Transition(
                "Jump→Idle",
                IdleState,
                () => JumpState.HasLanded));

            stateMachine.Initialise(IdleState);
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            stateMachine.PhysicsUpdate();
        }

        public void OnHit()
        {
            // Will be implemented Day 3 with combat states
        }

        public void OnDeath()
        {
            // Will be implemented Day 3
        }
    }
}