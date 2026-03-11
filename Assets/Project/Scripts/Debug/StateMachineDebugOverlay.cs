using ActionCombat.Player;
using UnityEngine;

namespace ActionCombat.Debug
{
    /// <summary>
    /// Runtime debug overlay visible in Game view.
    /// Toggle with F1 key.
    /// </summary>
    public class StateMachineDebugOverlay : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool showOnStart = true;

        private bool isVisible;
        private GUIStyle headerStyle;
        private GUIStyle stateStyle;
        private GUIStyle logStyle;
        private GUIStyle bgStyle;
        private bool stylesInitialised;

        private void Start()
        {
            isVisible = showOnStart;

            if (player == null)
                player = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey))
                isVisible = !isVisible;
        }

        private void InitStyles()
        {
            if (stylesInitialised) return;

            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.75f));
            bgTex.Apply();

            bgStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan },
                richText = true
            };

            stateStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                richText = true
            };

            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            stylesInitialised = true;
        }

        private void OnGUI()
        {
            if (!isVisible || player == null) return;

            InitStyles();

            var sm = player.DebugStateMachine;
            if (sm == null) return;

            float panelWidth = 320f;
            float panelHeight = 400f;
            float x = Screen.width - panelWidth - 10;
            float y = 10;

            GUI.Box(new Rect(x - 5, y - 5, panelWidth + 10, panelHeight + 10), "", bgStyle);

            GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight));

            GUILayout.Label("STATE MACHINE DEBUG [F1]", headerStyle);
            GUILayout.Space(5);

            string stateName = sm.CurrentState?.Name ?? "None";
            string prevState = sm.PreviousState?.Name ?? "None";
            GUILayout.Label($"Current:  <color=#00FF88>{stateName}</color>", stateStyle);
            GUILayout.Label($"Previous: {prevState}", stateStyle);
            GUILayout.Label($"Time in State: {sm.TimeInCurrentState:F2}s", stateStyle);

            GUILayout.Space(5);

            GUILayout.Label("── Player ──", headerStyle);
            GUILayout.Label($"Speed: {player.Movement.CurrentSpeed:F1} | Grounded: {player.Movement.IsGrounded}", stateStyle);
            GUILayout.Label($"Move Input: ({player.Input.MoveInput.x:F2}, {player.Input.MoveInput.y:F2})", stateStyle);
            GUILayout.Label($"Blocking: {player.Input.IsBlocking} | Buffered Attack: {player.Input.HasBufferedAttack}", stateStyle);

            GUILayout.Space(5);

            GUILayout.Label("── Transitions ──", headerStyle);
            if (sm.CurrentState is ActionCombat.Core.StateMachine.State state)
            {
                foreach (var t in state.Transitions)
                {
                    bool active = t.Condition();
                    string color = active ? "#00FF00" : "#FF4444";
                    GUILayout.Label($"  <color={color}>● {t.Name}</color>", stateStyle);
                }
            }

            GUILayout.Space(5);

            GUILayout.Label("── History ──", headerStyle);
            var history = sm.TransitionHistory;
            int start = Mathf.Max(0, history.Count - 8);
            for (int i = history.Count - 1; i >= start; i--)
            {
                GUILayout.Label($"  {history[i]}", logStyle);
            }

            GUILayout.EndArea();
        }
    }
}