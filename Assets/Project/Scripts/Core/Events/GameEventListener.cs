using UnityEngine;
using UnityEngine.Events;

namespace ActionCombat.Core.Events
{
    /// <summary>
    /// Attach to any GameObject to listen for a GameEvent.
    /// Wire the response in the Inspector via UnityEvent.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The GameEvent ScriptableObject to listen to")]
        [SerializeField] private GameEvent gameEvent;

        [Tooltip("Response to invoke when the event is raised")]
        [SerializeField] private UnityEvent response;

        private void OnEnable()
        {
            if (gameEvent != null)
                gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (gameEvent != null)
                gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            response?.Invoke();
        }
    }
}