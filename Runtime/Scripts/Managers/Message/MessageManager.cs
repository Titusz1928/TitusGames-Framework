using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TitusGames.Framework
{
public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    [Header("Hierarchy Containers")]
    public RectTransform messageContainer;

    [Header("Resource Paths (Inside 'Resources/')")]
    [SerializeField] private string prefabFolderPath = "UI/Messaging";
    [SerializeField] private string iconFolderPath = "UI/Icons";

    [Header("Default Asset Names")]
    [SerializeField] private string defaultPrefabName = "DefaultMessagePrefab";
    [SerializeField] private string defaultIconName = "DefaultMessageIcon";

    [Header("Settings")]
    public float messageDuration = 3f;
    public float fadeSpeed = 2f;

    // Internal lookup libraries to avoid heavy dynamic loading mid-game
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    private Queue<MessageData> messageQueue = new Queue<MessageData>();
    private bool isProcessing = false;

    private class MessageData
    {
        public string text;
        public string iconName;
        public Sprite explicitIcon; // Used if a script passes a direct reference instead of a string
        public string customPrefabName;
        public bool isLocalized;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAssets();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-locate container in the new scene safely
        var containerObj = GameObject.Find("MessageContainer");
        if (containerObj != null)
            messageContainer = containerObj.GetComponent<RectTransform>();
    }

    private void InitializeAssets()
        {
            // 1. Cache the default prefab instantly using the package path resolution string
            string fullPrefabPath = $"{prefabFolderPath}/{defaultPrefabName}";
            GameObject defaultPrefab = Resources.Load<GameObject>(fullPrefabPath);
            
            // Fallback check: If standard package resolution fails, try loading directly by full package asset locator path string
            if (defaultPrefab == null)
            {
                defaultPrefab = Resources.Load<GameObject>($"Packages/com.titusgames.framework/Runtime/Resources/{fullPrefabPath}");
            }

            if (defaultPrefab != null) prefabCache[defaultPrefabName] = defaultPrefab;
            else Debug.LogError($"[MessageManager] CRITICAL: Default prefab not found at Resources/{fullPrefabPath}");

            // 2. Cache the default icon instantly
            string fullIconPath = $"{iconFolderPath}/{defaultIconName}";
            Sprite defaultIcon = Resources.Load<Sprite>(fullIconPath);
            
            if (defaultIcon == null)
            {
                defaultIcon = Resources.Load<Sprite>($"Packages/com.titusgames.framework/Runtime/Resources/{fullIconPath}");
            }

            if (defaultIcon != null) iconCache[defaultIconName] = defaultIcon;
            else Debug.LogWarning($"[MessageManager] Default icon not found at Resources/{fullIconPath}");
        }

    // --- Dynamic Asset Resolution Core ---

    private GameObject GetPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) prefabName = defaultPrefabName;

            if (prefabCache.TryGetValue(prefabName, out GameObject cachedPrefab)) return cachedPrefab;

            string path = $"{prefabFolderPath}/{prefabName}";
            GameObject loadedPrefab = Resources.Load<GameObject>(path);
            
            if (loadedPrefab == null)
            {
                loadedPrefab = Resources.Load<GameObject>($"Packages/com.titusgames.framework/Runtime/Resources/{path}");
            }

            if (loadedPrefab != null)
            {
                prefabCache[prefabName] = loadedPrefab;
                return loadedPrefab;
            }

            Debug.LogWarning($"[MessageManager] Custom prefab '{prefabName}' not found. Falling back to default.");
            return prefabCache.ContainsKey(defaultPrefabName) ? prefabCache[defaultPrefabName] : null;
        }

        private Sprite GetIcon(string name, Sprite explicitSprite)
        {
            if (explicitSprite != null) return explicitSprite;
            if (string.IsNullOrEmpty(name)) name = defaultIconName;

            if (iconCache.TryGetValue(name, out Sprite cachedSprite)) return cachedSprite;

            string path = $"{iconFolderPath}/{name}";
            Sprite loadedSprite = Resources.Load<Sprite>(path);
            
            if (loadedSprite == null)
            {
                loadedSprite = Resources.Load<Sprite>($"Packages/com.titusgames.framework/Runtime/Resources/{path}");
            }

            if (loadedSprite != null)
            {
                iconCache[name] = loadedSprite;
                return loadedSprite;
            }

            return iconCache.ContainsKey(defaultIconName) ? iconCache[defaultIconName] : null;
        }


    // --- Public API ---

    /// <summary> Shows a localized JSON key string using structural fallbacks. </summary>
    public void ShowMessage(string key, string iconName = "", string customPrefabName = "")
    {
        messageQueue.Enqueue(new MessageData
        {
            text = key,
            iconName = iconName,
            customPrefabName = customPrefabName,
            isLocalized = true
        });
        if (!isProcessing) StartCoroutine(ProcessQueue());
    }

    /// <summary> Shows raw text explicitly bypassing translation checks. </summary>
    public void ShowMessageDirectly(string message, string iconName = "", string customPrefabName = "")
    {
        messageQueue.Enqueue(new MessageData
        {
            text = message,
            iconName = iconName,
            customPrefabName = customPrefabName,
            isLocalized = false
        });
        if (!isProcessing) StartCoroutine(ProcessQueue());
    }

    /// <summary> Overload allowing direct Sprite object injection alongside folder names. </summary>
    public void ShowMessageWithSprite(string text, Sprite explicitIcon, string customPrefabName = "", bool useLocalization = true)
    {
        messageQueue.Enqueue(new MessageData
        {
            text = text,
            explicitIcon = explicitIcon,
            customPrefabName = customPrefabName,
            isLocalized = useLocalization
        });
        if (!isProcessing) StartCoroutine(ProcessQueue());
    }

    // --- Core Logic ---

    private IEnumerator ProcessQueue()
    {
        isProcessing = true;

        while (messageQueue.Count > 0)
        {
            MessageData data = messageQueue.Dequeue();
            yield return StartCoroutine(DisplayMessage(data));
        }

        isProcessing = false;
    }

    private IEnumerator DisplayMessage(MessageData data)
    {
        if (messageContainer == null)
        {
            Debug.LogError("[MessageManager] No active MessageContainer canvas zone registered to spawn elements into!");
            yield break;
        }

        // Resolve target prefab dynamically
        GameObject targetPrefab = GetPrefab(data.customPrefabName);
        if (targetPrefab == null) yield break;

        GameObject msgInstance = Instantiate(targetPrefab, messageContainer);
        CanvasGroup cg = msgInstance.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        // 1. Extract and Apply Text
        var textObj = msgInstance.transform.Find("MessageText");
        if (textObj != null)
        {
            if (data.isLocalized)
            {
                var loc = textObj.GetComponent<LocalizedText>();
                if (loc != null) loc.SetKey(data.text);
            }
            else
            {
                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = data.text;
            }
        }

        // 2. Extract and Apply Icon
        var iconObj = msgInstance.transform.Find("MessageIcon");
        if (iconObj != null)
        {
            Sprite resolvedIcon = GetIcon(data.iconName, data.explicitIcon);
            var img = iconObj.GetComponent<Image>();

            if (img != null && resolvedIcon != null)
            {
                img.enabled = true;
                img.sprite = resolvedIcon;
            }
            else if (img != null && resolvedIcon == null)
            {
                // If there's deliberately no icon found or fallback wanted, hide the component container
                img.enabled = false;
            }
        }

        // 3. Animation Sequences (Independent of pause scaling)
        yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f));
        yield return new WaitForSecondsRealtime(messageDuration);
        yield return StartCoroutine(FadeCanvasGroup(cg, 1f, 0f));

        Destroy(msgInstance);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        if (cg == null) yield break;
        float time = 0f;
        while (time < 1f)
        {
            time += Time.unscaledDeltaTime * fadeSpeed;
            cg.alpha = Mathf.Lerp(start, end, time);
            yield return null;
        }
        cg.alpha = end;
    }

    public void RegisterContainer(RectTransform container) => messageContainer = container;
}
}