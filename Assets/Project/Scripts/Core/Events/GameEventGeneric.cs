using System.Collections.Generic;
using UnityEngine;

namespace ActionCombat.Core.Events
{
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> listeners = new List<IGameEventListener<T>>();

#if UNITY_EDITOR
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private bool debugLog = false;
#endif

        public void Raise(T value)
        {
#if UNITY_EDITOR
            if (debugLog)
                UnityEngine.Debug.Log($"[GameEvent<{typeof(T).Name}>] {name} raised with value: {value}. Listeners: {listeners.Count}", this);
#endif
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener<T> listener)
        {
            listeners.Remove(listener);
        }
    }

    public interface IGameEventListener<T>
    {
        void OnEventRaised(T value);
    }

    [CreateAssetMenu(fileName = "NewFloatEvent", menuName = "ActionCombat/Events/Float Event")]
    public class FloatEvent : GameEvent<float> { }

    [CreateAssetMenu(fileName = "NewIntEvent", menuName = "ActionCombat/Events/Int Event")]
    public class IntEvent : GameEvent<int> { }

    [CreateAssetMenu(fileName = "NewVector3Event", menuName = "ActionCombat/Events/Vector3 Event")]
    public class Vector3Event : GameEvent<Vector3> { }
}