using UnityEngine;
using UnityEditor;
using ActionCombat.Combat;

namespace ActionCombat.EditorTools
{
    /// <summary>
    /// Editor Window for visually editing hitbox data.
    /// Select a GameObject with HitboxController, then use
    /// Scene view handles to position and resize hitboxes.
    /// Window > ActionCombat > Hitbox Editor
    /// </summary>
    public class HitboxEditorWindow : EditorWindow
    {
        private HitboxController selectedHitbox;
        private HitboxData editingData;
        private bool livePreview = true;
        private Color hitboxColor = new Color(1f, 0f, 0f, 0.3f);
        private Color hitboxWireColor = new Color(1f, 0.2f, 0.2f, 0.8f);

        [MenuItem("Window/ActionCombat/Hitbox Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<HitboxEditorWindow>("Hitbox Editor");
            window.minSize = new Vector2(300, 350);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                selectedHitbox = Selection.activeGameObject.GetComponent<HitboxController>();
                if (selectedHitbox == null)
                    selectedHitbox = Selection.activeGameObject.GetComponentInChildren<HitboxController>();
            }
            else
            {
                selectedHitbox = null;
            }
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("HITBOX EDITOR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (selectedHitbox == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with a HitboxController component.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Entity: {selectedHitbox.gameObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Hitbox data selector
            editingData = (HitboxData)EditorGUILayout.ObjectField(
                "Hitbox Data", editingData, typeof(HitboxData), false);

            if (editingData == null)
            {
                EditorGUILayout.HelpBox("Assign a HitboxData asset to edit.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);
            livePreview = EditorGUILayout.Toggle("Live Preview in Scene", livePreview);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("DAMAGE", EditorStyles.boldLabel);

            editingData.baseDamage = EditorGUILayout.FloatField("Base Damage", editingData.baseDamage);
            editingData.damageType = (DamageType)EditorGUILayout.EnumPopup("Damage Type", editingData.damageType);
            editingData.knockbackForce = EditorGUILayout.FloatField("Knockback Force", editingData.knockbackForce);
            editingData.staggerDuration = EditorGUILayout.FloatField("Stagger Duration", editingData.staggerDuration);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("SHAPE", EditorStyles.boldLabel);

            editingData.radius = EditorGUILayout.Slider("Radius", editingData.radius, 0.1f, 3f);
            editingData.offset = EditorGUILayout.Vector3Field("Offset", editingData.offset);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("PREVIEW COLORS", EditorStyles.boldLabel);
            hitboxColor = EditorGUILayout.ColorField("Fill Color", hitboxColor);
            hitboxWireColor = EditorGUILayout.ColorField("Wire Color", hitboxWireColor);

            // Quick presets
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("QUICK PRESETS", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Light Attack"))
            {
                editingData.baseDamage = 10f;
                editingData.damageType = DamageType.Light;
                editingData.knockbackForce = 3f;
                editingData.staggerDuration = 0.3f;
                editingData.radius = 0.8f;
                editingData.offset = new Vector3(0f, 1f, 1f);
            }

            if (GUILayout.Button("Heavy Attack"))
            {
                editingData.baseDamage = 25f;
                editingData.damageType = DamageType.Heavy;
                editingData.knockbackForce = 6f;
                editingData.staggerDuration = 0.5f;
                editingData.radius = 1.2f;
                editingData.offset = new Vector3(0f, 1f, 1.2f);
            }

            if (GUILayout.Button("Wide Sweep"))
            {
                editingData.baseDamage = 15f;
                editingData.damageType = DamageType.Light;
                editingData.knockbackForce = 4f;
                editingData.staggerDuration = 0.3f;
                editingData.radius = 1.8f;
                editingData.offset = new Vector3(0f, 1f, 0.5f);
            }

            EditorGUILayout.EndHorizontal();

            if (GUI.changed && editingData != null)
            {
                EditorUtility.SetDirty(editingData);
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!livePreview || selectedHitbox == null || editingData == null) return;

            Transform t = selectedHitbox.transform;
            Vector3 worldPos = t.TransformPoint(editingData.offset);

            // Draw filled sphere
            Handles.color = hitboxColor;
            Handles.SphereHandleCap(0, worldPos, Quaternion.identity,
                editingData.radius * 2f, EventType.Repaint);

            // Draw wire sphere
            Handles.color = hitboxWireColor;
            Handles.DrawWireDisc(worldPos, Vector3.up, editingData.radius);
            Handles.DrawWireDisc(worldPos, Vector3.forward, editingData.radius);
            Handles.DrawWireDisc(worldPos, Vector3.right, editingData.radius);

            // Draggable position handle
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, t.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editingData, "Move Hitbox");
                editingData.offset = t.InverseTransformPoint(newWorldPos);
                EditorUtility.SetDirty(editingData);
            }

            // Radius handle
            EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(Quaternion.identity, worldPos, editingData.radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editingData, "Resize Hitbox");
                editingData.radius = newRadius;
                EditorUtility.SetDirty(editingData);
            }

            // Label
            Handles.Label(worldPos + Vector3.up * (editingData.radius + 0.3f),
                $"{editingData.name}\nDmg: {editingData.baseDamage} | R: {editingData.radius:F2}",
                EditorStyles.boldLabel);
        }
    }
}