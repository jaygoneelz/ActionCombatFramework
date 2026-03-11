using System.Collections.Generic;
using UnityEngine;

namespace ActionCombat.Core.Events
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "ActionCombat/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

#if UNITY_EDITOR
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private bool debugLog = false;
#endif

        public void Raise()
        {
#if UNITY_EDITOR
            if (debugLog)
                UnityEngine.Debug.Log($"[GameEvent] {name} raised. Listeners: {listeners.Count}", this);
#endif
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}