using UnityEngine;


namespace TitusGames.Framework{
public class SandboxInitializer : MonoBehaviour
{
    void Awake()
    {
        InitializeGlobalManagers();
    }

    private void InitializeGlobalManagers()
    {
        // 1. Essential Navigation & UI Managers
        EnsureManager<SceneManagerEX>("SceneManager");
        EnsureManager<WindowManager>("WindowManager");

        // 2. Localization (Infrastructure)
        if (LocalizationManager.Instance == null)
        {
            var locObj = new GameObject("LocalizationManager");
            var locManager = locObj.AddComponent<LocalizationManager>();
            DontDestroyOnLoad(locObj);
            locManager.Initialize();
        }

        // 3. Audio (Infrastructure)
        if (AudioManager.Instance == null)
        {
            var audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioObj);
        }

        // 4. Messaging System (Global UI)
        InitializeMessageManager();

        Debug.Log("<color=cyan>[Sandbox] Global Infrastructure Initialized.</color>");
    }

    private void InitializeMessageManager()
    {
        if (MessageManager.Instance != null) return;

        var msgObj = new GameObject("MessageManager");
        var mm = msgObj.AddComponent<MessageManager>();
        DontDestroyOnLoad(msgObj);

        var container = GameObject.Find("MessageContainer");
        if (container != null)
            mm.messageContainer = container.GetComponent<RectTransform>();
    }

    private void EnsureManager<T>(string name) where T : MonoBehaviour
    {
        if (Object.FindFirstObjectByType<T>() == null)
        {
            var obj = new GameObject(name);
            obj.AddComponent<T>();
            DontDestroyOnLoad(obj);
        }
    }
}
}