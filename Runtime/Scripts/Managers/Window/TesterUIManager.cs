using UnityEngine;
using UnityEngine.InputSystem;
using TitusGames.Framework;

namespace TitusGames.Framework.Testing
{
    public class TesterUIManager : MonoBehaviour
    {
        [Header("Test UI Prefabs")]
        [SerializeField] private GameObject pauseMenuPrefab;

        private IWindowService windowService;
        private float lastWindowClosedTime = -999f;

        private void Start()
        {
            if (ServiceLocator.Current != null)
            {
                windowService = ServiceLocator.Current.Get<IWindowService>();
            }

            if (windowService == null)
            {
                Debug.LogError("[TesterUIManager] IWindowService could not be found via ServiceLocator!");
            }
            else
            {
                // Subscribe to the framework's window close event
                windowService.OnWindowClosed += OnWindowClosed;
            }
        }

        private void OnDestroy()
        {
            if (windowService != null)
            {
                windowService.OnWindowClosed -= OnWindowClosed;
            }
        }

        private void OnWindowClosed(GameObject closedWindow)
        {
            // Record the exact time the last window finished closing
            lastWindowClosedTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (windowService == null) return;

            // Listen for Escape key to open the pause menu when no windows are open.
            // (Closing is fully managed by WindowManager's built-in cancel chain).
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Prevent opening if a window was closed within the last 150ms (prevents race condition)
                if (!windowService.IsAnyWindowOpen && (Time.unscaledTime - lastWindowClosedTime > 0.15f))
                {
                    if (pauseMenuPrefab != null)
                    {
                        windowService.OpenWindow(pauseMenuPrefab);
                        Debug.Log("[TesterUIManager] Opened pause menu.");
                    }
                    else
                    {
                        Debug.LogWarning("[TesterUIManager] Pause menu prefab is not assigned!");
                    }
                }
            }
        }
    }
}