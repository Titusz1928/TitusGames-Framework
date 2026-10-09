using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TitusGames.Framework{

public class WindowManager : MonoBehaviour,IWindowService, ICancelInputHandler
{

    [Header("Window Parent")]
    public Transform uiRoot;  // Canvas content target

    [Header("Global Input Settings")]
    [SerializeField] private bool useUniversalKeyboardFallback = true;

    [Header("Dynamic Input Naming Configuration")]
    public string uiActionMapName = "UI";
    public string playerActionMapName = "Player";
    public string cancelActionPath = "UI/Cancel";

    [Header("Managed Overlays (Auto-Hides during open windows)")]
    [SerializeField] private List<GameObject> managedOverlays = new List<GameObject>();

    // Internal list used as an ordered stack for easy re-ordering and removal
    private readonly List<GameObject> windowStack = new List<GameObject>();
    
    // Maps instantiated window GameObjects back to their source prefabs
    private readonly Dictionary<GameObject, GameObject> prefabInstanceMap = new Dictionary<GameObject, GameObject>();

    // Snapshot storage for HUD/Overlay visibility before menus open
    private readonly Dictionary<GameObject, bool> overlayVisibilitySnapshot = new Dictionary<GameObject, bool>();

    private PlayerInput activePlayerInput;
    private InputAction uiCancelAction;
    private ICancelInputHandler nextHandler;



    public int WindowCount
    {
        get
        {
            CleanDeadReferences();
            return windowStack.Count;
        }
    }

    public bool IsAnyWindowOpen => WindowCount > 0;

    // --- Events ---
    public event Action<GameObject> OnWindowOpened;
    public event Action<GameObject> OnWindowClosed;


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindActiveCancelAction();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-discover the local UI canvas root if it matches standard naming
        var rootObj = GameObject.Find("UIRoot");
        if (rootObj != null) uiRoot = rootObj.transform;

        // Auto-discover any active scene-wide PlayerInput if none was explicitly pushed
        if (activePlayerInput == null)
        {
            var foundInput = FindFirstObjectByType<PlayerInput>();
            if (foundInput != null) RegisterPlayerInput(foundInput);
        }

        UpdateInputAndTimeState();
    }

    private void Update()
    {
        // UNIVERSAL FALLBACK: If no Input Action Asset is active/bound in the scene, 
        // read the physical keyboard direct hardware state so Escape key still works perfectly.
        if (useUniversalKeyboardFallback && activePlayerInput == null)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleCancel();
            }
        }
    }

        // --- Chain of Responsibility Control ---
        public void SetNextHandler(ICancelInputHandler next)
        {
            nextHandler = next;
        }

        public void RegisterPlayerInput(PlayerInput input)
        {
            UnbindActiveCancelAction();
            activePlayerInput = input;

            if (activePlayerInput != null)
            {
                uiCancelAction = activePlayerInput.actions.FindAction(cancelActionPath);
                if (uiCancelAction != null)
                {
                    uiCancelAction.performed += OnCancelPressed;
                }
            }

            UpdateInputAndTimeState();
        }

        private void UnbindActiveCancelAction()
        {
            if (uiCancelAction != null)
            {
                uiCancelAction.performed -= OnCancelPressed;
                uiCancelAction = null;
            }
        }

        private void OnCancelPressed(InputAction.CallbackContext context)
        {
            HandleCancel();
        }

        public bool HandleCancel()
        {
            if (IsAnyWindowOpen)
            {
                CloseTopWindow();
                return true;
            }

            if (nextHandler != null)
            {
                return nextHandler.HandleCancel();
            }

            return false;
        }

        // --- Core Window Operations ---

        /// <summary>
        /// Opens a window from a prefab. If the window is already open, brings it to front visually
        /// and moves it to the top of the stack instead of creating a duplicate.
        /// </summary>
        public GameObject OpenWindow(GameObject windowPrefab)
        {
            if (windowPrefab == null)
            {
                Debug.LogError("[WindowManager] Cannot open window: Prefab parameter is null!");
                return null;
            }

            CleanDeadReferences();

            // 1. PREVENT DUPLICATES: Check if window instance from this prefab already exists
            GameObject existingWindow = FindOpenWindowFromPrefab(windowPrefab);
            if (existingWindow != null)
            {
                // Re-order in internal stack: Move to top so Escape closes it first
                windowStack.Remove(existingWindow);
                windowStack.Add(existingWindow);

                // Re-order in UI Canvas visually
                existingWindow.transform.SetAsLastSibling();

                UpdateInputAndTimeState();
                OnWindowOpened?.Invoke(existingWindow);
                return existingWindow;
            }

            // Snapshot HUD overlays when opening the FIRST window
            if (windowStack.Count == 0)
            {
                SnapshotAndHideOverlays();
            }

            // 2. SPAWN NEW WINDOW
            if (uiRoot == null)
            {
                Debug.LogWarning("[WindowManager] UI Root is missing. Instantiating to scene root.");
            }

            GameObject newWindow = Instantiate(windowPrefab, uiRoot);

            // Map tracking link
            prefabInstanceMap[newWindow] = windowPrefab;
            windowStack.Add(newWindow);

            // Ensure rendering at front of Canvas hierarchy
            newWindow.transform.SetAsLastSibling();

            UpdateInputAndTimeState();
            OnWindowOpened?.Invoke(newWindow);

            return newWindow;
        }

        public void CloseTopWindow()
        {
            CleanDeadReferences();

            if (windowStack.Count == 0) return;

            int lastIndex = windowStack.Count - 1;
            GameObject topWindow = windowStack[lastIndex];
            windowStack.RemoveAt(lastIndex);

            if (topWindow != null)
            {
                prefabInstanceMap.Remove(topWindow);
                OnWindowClosed?.Invoke(topWindow);
                Destroy(topWindow);
            }

            UpdateInputAndTimeState();
        }

        public GameObject GetTopWindow()
        {
            CleanDeadReferences();
            return windowStack.Count > 0 ? windowStack[windowStack.Count - 1] : null;
        }


        public void CloseAllWindows()
        {
            CleanDeadReferences();

            for (int i = windowStack.Count - 1; i >= 0; i--)
            {
                GameObject win = windowStack[i];
                if (win != null)
                {
                    prefabInstanceMap.Remove(win);
                    OnWindowClosed?.Invoke(win);
                    Destroy(win);
                }
            }

            windowStack.Clear();
            overlayVisibilitySnapshot.Clear();

            UpdateInputAndTimeState();
        }

        public void RegisterUIRoot(Transform root)
        {
            uiRoot = root;
        }

        // --- HUD / Overlay Snapshot System ---

        /// <summary>
        /// Registers a HUD overlay object (e.g. gameplay minimap, quest tracker) to automatically hide when windows open.
        /// </summary>
        public void RegisterManagedOverlay(GameObject overlay)
        {
            if (overlay != null && !managedOverlays.Contains(overlay))
            {
                managedOverlays.Add(overlay);
            }
        }

        private void SnapshotAndHideOverlays()
        {
            overlayVisibilitySnapshot.Clear();

            foreach (var overlay in managedOverlays)
            {
                if (overlay != null)
                {
                    overlayVisibilitySnapshot[overlay] = overlay.activeSelf;
                    overlay.SetActive(false);
                }
            }
        }

        private void RestoreOverlays()
        {
            foreach (var kvp in overlayVisibilitySnapshot)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.SetActive(kvp.Value);
                }
            }

            overlayVisibilitySnapshot.Clear();
        }

        // --- Helper Internals ---

        private GameObject FindOpenWindowFromPrefab(GameObject prefab)
        {
            foreach (var window in windowStack)
            {
                if (window == null) continue;

                // Check 1: Dictionary lookup
                if (prefabInstanceMap.TryGetValue(window, out var sourcePrefab) && sourcePrefab == prefab)
                {
                    return window;
                }

                // Check 2: Fallback name match if dictionary lookup missed
                if (window.name == prefab.name || window.name == $"{prefab.name}(Clone)")
                {
                    return window;
                }
            }

            return null;
        }

        private void CleanDeadReferences()
        {
            for (int i = windowStack.Count - 1; i >= 0; i--)
            {
                if (windowStack[i] == null)
                {
                    windowStack.RemoveAt(i);
                }
            }
        }

        private void UpdateInputAndTimeState()
        {
            CleanDeadReferences();

            bool needsUIOnly = false;
            bool needsTimeFreeze = false;
            bool cursorShouldShow = false;

            if (IsAnyWindowOpen)
            {
                // Evaluate settings across active windows
                foreach (GameObject window in windowStack)
                {
                    if (window != null && window.TryGetComponent(out UIWindowSettings settings))
                    {
                        if (settings.inputMode == WindowInputMode.UIOnly) needsUIOnly = true;
                        if (settings.freezeTime) needsTimeFreeze = true;
                        if (settings.showCursor) cursorShouldShow = true;
                    }
                }
            }
            else
            {
                // Restore hidden HUD overlays when last window is closed
                RestoreOverlays();
            }

            // Apply Time System State
            Time.timeScale = needsTimeFreeze ? 0f : 1f;

            // Apply Cursor Configuration
            if (IsAnyWindowOpen)
            {
                Cursor.visible = cursorShouldShow;
                Cursor.lockState = cursorShouldShow ? CursorLockMode.None : CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Apply Action Map State Switches
            if (activePlayerInput != null)
            {
                var uiMap = activePlayerInput.actions.FindActionMap(uiActionMapName);
                if (uiMap != null && !uiMap.enabled) uiMap.Enable();

                var playerMap = activePlayerInput.actions.FindActionMap(playerActionMapName);
                if (playerMap != null)
                {
                    if (needsUIOnly) playerMap.Disable();
                    else playerMap.Enable();
                }
            }
        }

        private void OnDestroy()
        {
        UnbindActiveCancelAction();
        }
    }
}
