using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TitusGames.Framework{

public class WindowManager : MonoBehaviour, ICancelInputHandler
{
    public static WindowManager Instance { get; private set; }

    [Header("Window Parent")]
    public Transform uiRoot;  // Canvas content target

    [Header("Global Input Settings")]
    [Tooltip("If true, Escape key on keyboard will always attempt to close top window if no custom PlayerInput is registered.")]
    [SerializeField] private bool useUniversalKeyboardFallback = true;

    [Header("Dynamic Input Naming Configuration")]
    [Tooltip("The name of your UI/Interface Action Map.")]
    public string uiActionMapName = "UI";
        
    [Tooltip("The name of your gameplay/character Action Map.")]
    public string playerActionMapName = "Player";
        
    [Tooltip("The full action map path to bind for canceling/closing windows.")]
    public string cancelActionPath = "UI/Cancel";

    private Stack<GameObject> windowStack = new Stack<GameObject>();
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

    public event Action OnWindowClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

    // --- Input Binding Lifecycle ---
    public void RegisterPlayerInput(PlayerInput input)
    {
        UnbindActiveCancelAction();

        activePlayerInput = input;

        if (activePlayerInput != null)
        {
            // Look for standard UI layout Cancel action path
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
        // Step 1: Handle open window layer hierarchies
        if (IsAnyWindowOpen)
        {
            CloseTopWindow();
            return true; // Input fully consumed!
        }

        // Step 2: Pass down the chain to other game state handlers (e.g. gameplay pause menus)
        if (nextHandler != null)
        {
            return nextHandler.HandleCancel();
        }

        return false;
    }

    // --- Core Window Operations ---
    public GameObject OpenWindow(GameObject windowPrefab)
    {
        if (windowPrefab == null)
        {
            Debug.LogError("[WindowManager] Cannot open window: Prefab parameter is Null!");
            return null;
        }

        if (uiRoot == null)
        {
            Debug.LogWarning("[WindowManager] UI Root target transform is missing. Instantiating to scene root.");
        }

        GameObject window = Instantiate(windowPrefab, uiRoot);
        windowStack.Push(window);

        UpdateInputAndTimeState();
        return window;
    }

    public void CloseTopWindow()
    {
        CleanDeadReferences();

        if (windowStack.Count == 0) return;

        GameObject topWindow = windowStack.Pop();
        if (topWindow != null)
        {
            Destroy(topWindow);
        }

        UpdateInputAndTimeState();

        // Fire merged tracking notifier safely
        OnWindowClosed?.Invoke();
    }

    public GameObject GetTopWindow()
    {
        CleanDeadReferences();
        return windowStack.Count > 0 ? windowStack.Peek() : null;
    }


    public void CloseAllWindows()
    {
        while (windowStack.Count > 0)
        {
            GameObject win = windowStack.Pop();
            if (win != null) Destroy(win);
        }

        UpdateInputAndTimeState();
    }

    public void RegisterUIRoot(Transform root)
    {
        uiRoot = root;
    }

    // --- State Calculation Internals ---
    private void CleanDeadReferences()
    {
        while (windowStack.Count > 0 && windowStack.Peek() == null)
        {
            windowStack.Pop();
        }
    }

    private void UpdateInputAndTimeState()
    {
        CleanDeadReferences();

        bool needsUIOnly = false;
        bool needsTimeFreeze = false;
        bool cursorShouldShow = false;

        // Evaluate collective settings rules across active windows
        foreach (GameObject window in windowStack)
        {
            if (window != null && window.TryGetComponent(out UIWindowSettings settings))
            {
                if (settings.inputMode == WindowInputMode.UIOnly) needsUIOnly = true;
                if (settings.freezeTime) needsTimeFreeze = true;
                if (settings.showCursor) cursorShouldShow = true;
            }
        }

        // Apply Time System State
        Time.timeScale = needsTimeFreeze ? 0f : 1f;

        // Apply Cursor Configuration Safely
        if (IsAnyWindowOpen)
        {
            Cursor.visible = cursorShouldShow;
            Cursor.lockState = cursorShouldShow ? CursorLockMode.None : CursorLockMode.Locked;
        }
        else
        {
            // Reset to defaults if everything is closed
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Apply Action Map State Switches
        if (activePlayerInput != null)
            {
                // Dynamic Map Name switches replace hardcoded "UI" and "Player" strings
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
