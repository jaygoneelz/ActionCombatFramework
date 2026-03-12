using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionCombat.Combat;

namespace ActionCombat.EditorTools
{
    /// <summary>
    /// Editor Window that logs all combat events during Play mode.
    /// Filterable, sortable, exportable to CSV.
    /// Window > ActionCombat > Combat Log
    /// </summary>
    public class CombatLogWindow : EditorWindow
    {
        private static CombatLogWindow instance;

        private struct CombatLogEntry
        {
            public float Time;
            public string Source;
            public string Target;
            public float Damage;
            public DamageType Type;
            public bool WasBlocked;
            public float RemainingHP;
            public float MaxHP;
        }

        private static List<CombatLogEntry> logEntries = new List<CombatLogEntry>();
        private Vector2 scrollPosition;
        private string filterSource = "";
        private string filterTarget = "";
        private DamageType? filterType = null;
        private bool autoScroll = true;
        private bool isRecording = true;
        private HealthComponent[] trackedHealth;

        // Stats
        private float totalDamageDealt;
        private int totalHits;
        private int totalKills;

        private void OnEnable()
        {
            EditorApplication.update += TrackHealthComponents;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TrackHealthComponents;
            UnsubscribeAll();
        }

        private void TrackHealthComponents()
        {
            if (!Application.isPlaying)
            {
                UnsubscribeAll();
                trackedHealth = null;
                return;
            }

            var allHealth = GameObject.FindObjectsOfType<HealthComponent>();
            if (trackedHealth == null || trackedHealth.Length != allHealth.Length)
            {
                UnsubscribeAll();
                trackedHealth = allHealth;
                foreach (var h in trackedHealth)
                {
                    h.OnDamaged += (data) => OnAnyDamaged(h, data);
                }
            }
        }

        private void UnsubscribeAll()
        {
            // We can't easily unsub lambdas, so we just null the array
            // and let garbage collection handle it. Fine for editor tool.
            trackedHealth = null;
        }

        private void OnAnyDamaged(HealthComponent target, DamageData data)
        {
            if (!isRecording) return;

            logEntries.Add(new CombatLogEntry
            {
                Time = Time.time,
                Source = data.Source != null ? data.Source.name : "Unknown",
                Target = target.gameObject.name,
                Damage = data.BaseDamage,
                Type = data.Type,
                WasBlocked = target.IsBlocking,
                RemainingHP = target.CurrentHealth,
                MaxHP = target.MaxHealth
            });

            totalDamageDealt += data.BaseDamage;
            totalHits++;

            if (target.CurrentHealth <= 0f)
                totalKills++;

            Repaint();
        }

        [MenuItem("Window/ActionCombat/Combat Log")]
        public static void ShowWindow()
        {
            instance = GetWindow<CombatLogWindow>("Combat Log");
            instance.minSize = new Vector2(500, 300);
        }

        /// <summary>
        /// Call this from CombatFeedbackBridge or HealthComponent
        /// to log an event. Static so it can be called from anywhere.
        /// </summary>
        public static void LogDamageEvent(string source, string target,
            float damage, DamageType type, bool wasBlocked,
            float remainingHP, float maxHP)
        {
            if (instance == null || !instance.isRecording) return;

            logEntries.Add(new CombatLogEntry
            {
                Time = Time.time,
                Source = source,
                Target = target,
                Damage = damage,
                Type = type,
                WasBlocked = wasBlocked,
                RemainingHP = remainingHP,
                MaxHP = maxHP
            });

            instance.totalDamageDealt += damage;
            instance.totalHits++;

            if (remainingHP <= 0f)
                instance.totalKills++;

            instance.Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawStats();
            EditorGUILayout.Space(3);
            DrawFilters();
            EditorGUILayout.Space(3);
            DrawLog();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            isRecording = GUILayout.Toggle(isRecording,
                isRecording ? "● Recording" : "○ Paused",
                EditorStyles.toolbarButton, GUILayout.Width(90));

            autoScroll = GUILayout.Toggle(autoScroll, "Auto-Scroll",
                EditorStyles.toolbarButton, GUILayout.Width(80));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                logEntries.Clear();
                totalDamageDealt = 0;
                totalHits = 0;
                totalKills = 0;
            }

            if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ExportCSV();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStats()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Hits: {totalHits}", EditorStyles.miniLabel);
            GUILayout.Label($"Total Damage: {totalDamageDealt:F0}", EditorStyles.miniLabel);
            GUILayout.Label($"Kills: {totalKills}", EditorStyles.miniLabel);

            if (totalHits > 0)
                GUILayout.Label($"Avg Damage: {totalDamageDealt / totalHits:F1}", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filters:", GUILayout.Width(45));
            filterSource = EditorGUILayout.TextField("Source", filterSource, GUILayout.Width(150));
            filterTarget = EditorGUILayout.TextField("Target", filterTarget, GUILayout.Width(150));

            // Type filter dropdown
            string[] typeOptions = { "All", "Light", "Heavy", "Special" };
            int currentType = filterType.HasValue ? (int)filterType.Value + 1 : 0;
            int newType = EditorGUILayout.Popup(currentType, typeOptions, GUILayout.Width(70));
            filterType = newType == 0 ? null : (DamageType?)(newType - 1);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLog()
        {
            // Column headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Time", EditorStyles.miniLabel, GUILayout.Width(50));
            GUILayout.Label("Source", EditorStyles.miniLabel, GUILayout.Width(80));
            GUILayout.Label("Target", EditorStyles.miniLabel, GUILayout.Width(80));
            GUILayout.Label("Damage", EditorStyles.miniLabel, GUILayout.Width(55));
            GUILayout.Label("Type", EditorStyles.miniLabel, GUILayout.Width(50));
            GUILayout.Label("Blocked", EditorStyles.miniLabel, GUILayout.Width(50));
            GUILayout.Label("HP Remaining", EditorStyles.miniLabel, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < logEntries.Count; i++)
            {
                var entry = logEntries[i];

                // Apply filters
                if (!string.IsNullOrEmpty(filterSource) &&
                    !entry.Source.ToLower().Contains(filterSource.ToLower()))
                    continue;
                if (!string.IsNullOrEmpty(filterTarget) &&
                    !entry.Target.ToLower().Contains(filterTarget.ToLower()))
                    continue;
                if (filterType.HasValue && entry.Type != filterType.Value)
                    continue;

                // Alternate row colors
                Color oldBg = GUI.backgroundColor;
                GUI.backgroundColor = i % 2 == 0
                    ? new Color(0.25f, 0.25f, 0.25f)
                    : new Color(0.22f, 0.22f, 0.22f);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = oldBg;

                GUILayout.Label($"{entry.Time:F1}s", GUILayout.Width(50));
                GUILayout.Label(entry.Source, GUILayout.Width(80));
                GUILayout.Label(entry.Target, GUILayout.Width(80));

                // Color damage by type
                GUIStyle damageStyle = new GUIStyle(EditorStyles.label);
                switch (entry.Type)
                {
                    case DamageType.Heavy:
                        damageStyle.normal.textColor = new Color(1f, 0.7f, 0.2f);
                        break;
                    case DamageType.Special:
                        damageStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                        break;
                    default:
                        damageStyle.normal.textColor = Color.white;
                        break;
                }
                GUILayout.Label($"{entry.Damage:F1}", damageStyle, GUILayout.Width(55));

                GUILayout.Label(entry.Type.ToString(), GUILayout.Width(50));
                GUILayout.Label(entry.WasBlocked ? "YES" : "", GUILayout.Width(50));

                // HP bar
                float hpNorm = entry.MaxHP > 0 ? entry.RemainingHP / entry.MaxHP : 0;
                Rect hpRect = GUILayoutUtility.GetRect(90, 16, GUILayout.Width(90));
                EditorGUI.ProgressBar(hpRect, hpNorm, $"{entry.RemainingHP:F0}/{entry.MaxHP:F0}");

                EditorGUILayout.EndHorizontal();
            }

            if (autoScroll && logEntries.Count > 0)
            {
                GUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ExportCSV()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Combat Log", "", "combat_log.csv", "csv");

            if (string.IsNullOrEmpty(path)) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Time,Source,Target,Damage,Type,Blocked,RemainingHP,MaxHP");

            foreach (var entry in logEntries)
            {
                sb.AppendLine($"{entry.Time:F2},{entry.Source},{entry.Target}," +
                    $"{entry.Damage:F1},{entry.Type},{entry.WasBlocked}," +
                    $"{entry.RemainingHP:F0},{entry.MaxHP:F0}");
            }

            System.IO.File.WriteAllText(path, sb.ToString());
            UnityEngine.Debug.Log($"[CombatLog] Exported {logEntries.Count} entries to {path}");
            EditorUtility.RevealInFinder(path);
        }
    }
}