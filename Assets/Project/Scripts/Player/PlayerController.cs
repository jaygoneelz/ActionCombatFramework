using ActionCombat.Core.StateMachine;
using ActionCombat.Player.States;
using UnityEngine;
using SM = ActionCombat.Core.StateMachine.StateMachine;

namespace ActionCombat.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerMovement Movement { get; private set; }
        public PlayerInputHandler Input { get; private set; }
        public PlayerAnimator Animator { get; private set; }

        private SM stateMachine;

        // All states — public for debug access
        public IdleState IdleState { get; private set; }
        public RunState RunState { get; private set; }
        public JumpState JumpState { get; private set; }
        public LightAttackState LightAttackState { get; private set; }
        public HeavyAttackState HeavyAttackState { get; private set; }
        public DodgeState DodgeState { get; private set; }
        public BlockState BlockState { get; private set; }

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

            // --- Create all states ---
            IdleState = new IdleState(stateMachine, this);
            RunState = new RunState(stateMachine, this);
            JumpState = new JumpState(stateMachine, this);
            LightAttackState = new LightAttackState(stateMachine, this);
            HeavyAttackState = new HeavyAttackState(stateMachine, this);
            DodgeState = new DodgeState(stateMachine, this);
            BlockState = new BlockState(stateMachine, this);

            // ============================================================
            // TRANSITION RULES
            // Priority = order added. First match wins.
            // Pattern: Dodge > Block > Attack > Jump > Move > Idle
            // ============================================================

            // --- IDLE transitions ---
            IdleState.AddTransition(new Transition(
                "Idle→Dodge", DodgeState,
                () => Input.ConsumeDodge()));

            IdleState.AddTransition(new Transition(
                "Idle→Block", BlockState,
                () => Input.IsBlocking));

            IdleState.AddTransition(new Transition(
                "Idle→LightAttack", LightAttackState,
                () => Input.ConsumeLightAttack()));

            IdleState.AddTransition(new Transition(
                "Idle→HeavyAttack", HeavyAttackState,
                () => Input.ConsumeHeavyAttack()));

            IdleState.AddTransition(new Transition(
                "Idle→Jump", JumpState,
                () => Input.ConsumeJump() && Movement.IsGrounded));

            IdleState.AddTransition(new Transition(
                "Idle→Run", RunState,
                () => Input.MoveInput.sqrMagnitude > 0.01f));

            // --- RUN transitions ---
            RunState.AddTransition(new Transition(
                "Run→Dodge", DodgeState,
                () => Input.ConsumeDodge()));

            RunState.AddTransition(new Transition(
                "Run→Block", BlockState,
                () => Input.IsBlocking));

            RunState.AddTransition(new Transition(
                "Run→LightAttack", LightAttackState,
                () => Input.ConsumeLightAttack()));

            RunState.AddTransition(new Transition(
                "Run→HeavyAttack", HeavyAttackState,
                () => Input.ConsumeHeavyAttack()));

            RunState.AddTransition(new Transition(
                "Run→Jump", JumpState,
                () => Input.ConsumeJump() && Movement.IsGrounded));

            RunState.AddTransition(new Transition(
                "Run→Idle", IdleState,
                () => Input.MoveInput.sqrMagnitude < 0.01f));

            // --- JUMP transitions ---
            JumpState.AddTransition(new Transition(
                "Jump→Idle", IdleState,
                () => JumpState.HasLanded));

            // --- LIGHT ATTACK transitions ---
            LightAttackState.AddTransition(new Transition(
                "LightAttack→Dodge", DodgeState,
                () => LightAttackState.IsComplete && Input.ConsumeDodge()));

            LightAttackState.AddTransition(new Transition(
                "LightAttack→Idle", IdleState,
                () => LightAttackState.IsComplete));

            // --- HEAVY ATTACK transitions ---
            HeavyAttackState.AddTransition(new Transition(
                "HeavyAttack→Dodge", DodgeState,
                () => HeavyAttackState.IsComplete && Input.ConsumeDodge()));

            HeavyAttackState.AddTransition(new Transition(
                "HeavyAttack→Idle", IdleState,
                () => HeavyAttackState.IsComplete));

            // --- DODGE transitions ---
            DodgeState.AddTransition(new Transition(
                "Dodge→LightAttack", LightAttackState,
                () => DodgeState.IsComplete && Input.ConsumeLightAttack()));

            DodgeState.AddTransition(new Transition(
                "Dodge→Idle", IdleState,
                () => DodgeState.IsComplete));

            // --- BLOCK transitions ---
            BlockState.AddTransition(new Transition(
                "Block→Idle", IdleState,
                () => !Input.IsBlocking));

            // --- Start ---
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
            // Day 3: will force stagger state
        }

        public void OnDeath()
        {
            // Day 3: will force death state
        }
    }
}