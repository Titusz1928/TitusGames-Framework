using UnityEngine;

namespace TitusGames.Framework
{
    [AddComponentMenu("TitusGames/Framework/Global Cancel Handler")]
    public class GlobalCancelHandler : MonoBehaviour, ICancelInputHandler
    {
        [Header("Chain Settings")]
        [Tooltip("The system window prefab (e.g., Pause Menu) to spawn when Cancel is pressed and no windows are open.")]
        [SerializeField] private GameObject primarySystemMenuPrefab;

        private GameObject activeMenuInstance;

        private void Start()
        {
            if (WindowManager.Instance != null)
            {
                WindowManager.Instance.SetNextHandler(this);
            }
        }

        public bool HandleCancel()
        {
            // If the system menu isn't open yet, spawn it through the WindowManager stack
            if (activeMenuInstance == null && primarySystemMenuPrefab != null)
            {
                activeMenuInstance = WindowManager.Instance.OpenWindow(primarySystemMenuPrefab);
                return true; // Input completely handled/consumed
            }

            return false; // Allow remaining systems further down the line to receive fallback events
        }
    }
}