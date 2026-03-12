using UnityEngine;
using UnityEditor;
using ActionCombat.AI;

namespace ActionCombat.EditorTools
{
    /// <summary>
    /// Draws debug gizmos for all enemies in Scene view:
    /// detection range, attack range, current state, target line.
    /// </summary>
    [CustomEditor(typeof(EnemyController))]
    public class EnemyDebugGizmos : Editor
    {
        private void OnSceneGUI()
        {
            EnemyController enemy = (EnemyController)target;

            // Detection range (yellow)
            Handles.color = new Color(1f, 1f, 0f, 0.08f);
            Handles.DrawSolidDisc(enemy.transform.position, Vector3.up, enemy.DetectionRange);
            Handles.color = new Color(1f, 1f, 0f, 0.4f);
            Handles.DrawWireDisc(enemy.transform.position, Vector3.up, enemy.DetectionRange);

            // Attack range (red)
            Handles.color = new Color(1f, 0f, 0f, 0.1f);
            Handles.DrawSolidDisc(enemy.transform.position, Vector3.up, enemy.AttackRange);
            Handles.color = new Color(1f, 0f, 0f, 0.5f);
            Handles.DrawWireDisc(enemy.transform.position, Vector3.up, enemy.AttackRange);

            // State label
            if (Application.isPlaying && enemy.DebugStateMachine != null)
            {
                string stateName = enemy.DebugStateMachine.CurrentState?.Name ?? "None";
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    normal = { textColor = Color.cyan },
                    alignment = TextAnchor.MiddleCenter
                };

                Handles.Label(enemy.transform.position + Vector3.up * 3f, stateName, style);

                // Line to target
                if (enemy.Target != null)
                {
                    Handles.color = Color.red;
                    Handles.DrawDottedLine(
                        enemy.transform.position + Vector3.up,
                        enemy.Target.position + Vector3.up,
                        4f);

                    // Distance label at midpoint
                    Vector3 midpoint = (enemy.transform.position + enemy.Target.position) * 0.5f + Vector3.up;
                    float dist = enemy.DistanceToTarget();
                    Handles.Label(midpoint, $"{dist:F1}m", EditorStyles.boldLabel);
                }
            }
        }
    }
}