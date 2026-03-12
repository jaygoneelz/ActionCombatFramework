using UnityEngine;
using UnityEditor;
using ActionCombat.Player;
using ActionCombat.AI;
using ActionCombat.Core.StateMachine;
using SM = ActionCombat.Core.StateMachine.StateMachine;

namespace ActionCombat.EditorTools
{
    /// <summary>
    /// Dockable Editor Window: live state machine visualization.
    /// Shows current state, transitions, history, and timing.
    /// Window > ActionCombat > State Machine Debugger
    /// </summary>
    public class StateMachineDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private SM trackedStateMachine;
        private string trackedName = "None";
        private bool autoTrackSelection = true;
        private bool showTransitions = true;
        private bool showHistory = true;
        private int maxHistoryDisplay = 15;

        // Styles
        private GUIStyle headerStyle;
        private GUIStyle stateBoxStyle;
        private GUIStyle activeTransitionStyle;
        private GUIStyle inactiveTransitionStyle;
        private bool stylesInitialised;

        [MenuItem("Window/ActionCombat/State Machine Debugger")]
        public static void ShowWindow()
        {
            var window = GetWindow<StateMachineDebugWindow>("SM Debugger");
            window.minSize = new Vector2(320, 400);
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void InitStyles()
        {
            if (stylesInitialised) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.4f, 0.8f, 1f) }
            };

            stateBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fixedHeight = 50
            };

            activeTransitionStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.3f, 1f, 0.3f) },
                fontStyle = FontStyle.Bold
            };

            inactiveTransitionStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };

            stylesInitialised = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            autoTrackSelection = GUILayout.Toggle(autoTrackSelection, "Auto-Track Selection",
                EditorStyles.toolbarButton, GUILayout.Width(130));
            showTransitions = GUILayout.Toggle(showTransitions, "Transitions",
                EditorStyles.toolbarButton, GUILayout.Width(80));
            showHistory = GUILayout.Toggle(showHistory, "History",
                EditorStyles.toolbarButton, GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Auto-track selected GameObject
            if (autoTrackSelection && Selection.activeGameObject != null)
            {
                TryTrackGameObject(Selection.activeGameObject);
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug state machines.\n\nSelect a GameObject with PlayerController or EnemyController.", MessageType.Info);
                return;
            }

            if (trackedStateMachine == null)
            {
                EditorGUILayout.HelpBox("No state machine tracked.\n\nSelect a Player or Enemy in the Hierarchy.", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(5);
            DrawCurrentState();
            EditorGUILayout.Space(5);

            if (showTransitions)
            {
                DrawTransitions();
                EditorGUILayout.Space(5);
            }

            if (showHistory)
            {
                DrawHistory();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("TRACKING", headerStyle);
            EditorGUILayout.LabelField($"Entity: {trackedName}", EditorStyles.boldLabel);
        }

        private void DrawCurrentState()
        {
            EditorGUILayout.LabelField("CURRENT STATE", headerStyle);

            string currentName = trackedStateMachine.CurrentState?.Name ?? "None";
            string prevName = trackedStateMachine.PreviousState?.Name ?? "None";

            // Big state name box
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.3f);
            EditorGUILayout.LabelField(currentName, stateBoxStyle);
            GUI.backgroundColor = oldBg;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Previous:", GUILayout.Width(60));
            EditorGUILayout.LabelField(prevName, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time:", GUILayout.Width(60));
            EditorGUILayout.LabelField($"{trackedStateMachine.TimeInCurrentState:F2}s", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTransitions()
        {
            EditorGUILayout.LabelField("TRANSITIONS", headerStyle);

            if (trackedStateMachine.CurrentState is State state)
            {
                if (state.Transitions.Count == 0)
                {
                    EditorGUILayout.LabelField("  (no transitions from this state)");
                    return;
                }

                foreach (var t in state.Transitions)
                {
                    bool conditionMet = false;
                    try
                    {
                        conditionMet = t.Condition();
                    }
                    catch
                    {
                        // Condition evaluation may fail during frame gaps
                    }

                    EditorGUILayout.BeginHorizontal();

                    // Status indicator
                    GUIStyle style = conditionMet ? activeTransitionStyle : inactiveTransitionStyle;
                    string icon = conditionMet ? "● " : "○ ";
                    EditorGUILayout.LabelField(icon + t.Name, style);

                    // Target state
                    EditorGUILayout.LabelField("→ " + t.TargetState.Name,
                        GUILayout.Width(120));

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawHistory()
        {
            EditorGUILayout.LabelField("TRANSITION HISTORY", headerStyle);

            var history = trackedStateMachine.TransitionHistory;
            if (history.Count == 0)
            {
                EditorGUILayout.LabelField("  (no transitions yet)");
                return;
            }

            int start = Mathf.Max(0, history.Count - maxHistoryDisplay);
            for (int i = history.Count - 1; i >= start; i--)
            {
                var record = history[i];
                float age = Time.time - record.Time;

                // Fade older entries
                Color oldColor = GUI.color;
                float alpha = Mathf.Lerp(0.4f, 1f, 1f - (age / 10f));
                GUI.color = new Color(1f, 1f, 1f, Mathf.Max(0.3f, alpha));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{record.Time:F1}s]", GUILayout.Width(50));
                EditorGUILayout.LabelField($"{record.FromState} → {record.ToState}");
                EditorGUILayout.LabelField(record.Reason, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                GUI.color = oldColor;
            }
        }

        private void TryTrackGameObject(GameObject go)
        {
            // Try Player
            var player = go.GetComponent<PlayerController>();
            if (player != null)
            {
                trackedStateMachine = player.DebugStateMachine;
                trackedName = go.name + " (Player)";
                return;
            }

            // Try Enemy
            var enemy = go.GetComponent<EnemyController>();
            if (enemy != null)
            {
                trackedStateMachine = enemy.DebugStateMachine;
                trackedName = go.name + " (Enemy)";
                return;
            }
        }
    }
}