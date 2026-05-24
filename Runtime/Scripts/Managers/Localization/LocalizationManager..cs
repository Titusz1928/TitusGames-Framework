using System;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace TitusGames.Framework
{
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Localization Settings")]
    [Tooltip("Folder inside Resources where language json files live (e.g. Resources/Languages)")]
    public string resourcesFolder = "Languages";

    [Tooltip("Name of the manifest file tracking active language settings.")]
    public string manifestFileName = "languages_manifest";

   [Tooltip("Key used in PlayerPrefs for saving selected language index")]
    public string prefsKey = "languageIndex";

   // Dynamic lists resolved directly from the config file
    [HideInInspector] public List<string> languageCodes = new List<string>();
    [HideInInspector] public List<string> languageDisplayNames = new List<string>();

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();
    public static event Action OnLanguageChanged;

    public string CurrentLanguageCode { get; private set; } = "eng";

    // --- Singleton setup ---
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

        /// <summary>
        /// Reads configuration rules out of your project assets folder and boots languages.
        /// </summary>
        public void Initialize()
        {
            LoadManifestFile();

            if (languageCodes.Count > 0)
            {
                int savedIndex = PlayerPrefs.GetInt(prefsKey, 0);
                savedIndex = Mathf.Clamp(savedIndex, 0, languageCodes.Count - 1);
                LoadLanguage(languageCodes[savedIndex]);
            }
            else
            {
                Debug.LogWarning("[LocalizationManager] Initialization aborted: Manifest data missing or empty.");
            }
        }

        private void LoadManifestFile()
        {
            languageCodes.Clear();
            languageDisplayNames.Clear();

            string manifestPath = $"{resourcesFolder}/{manifestFileName}";
            TextAsset manifestAsset = Resources.Load<TextAsset>(manifestPath);

            if (manifestAsset == null)
            {
                Debug.LogError($"[LocalizationManager] CRITICAL: Manifest configuration file not found at 'Resources/{manifestPath}.json'.");
                return;
            }

            var jsonObject = Json.Deserialize(manifestAsset.text) as Dictionary<string, object>;
            if (jsonObject != null && jsonObject.TryGetValue("supportedLanguages", out object dataList))
            {
                var languages = dataList as List<object>;
                if (languages != null)
                {
                    foreach (object langData in languages)
                    {
                        string code = langData.ToString();
                        languageCodes.Add(code);

                        // Dynamic Display Name Lookup: Read the language file to find its name label
                        string langFilePath = $"{resourcesFolder}/{code}";
                        TextAsset langAsset = Resources.Load<TextAsset>(langFilePath);

                        if (langAsset != null)
                        {
                            var parsedLang = ParseFlatJsonToDictionary(langAsset.text);
                            // Read its localized identifier key, fallback to uppercase code if missing
                            if (parsedLang.TryGetValue(code, out string nativeName))
                            {
                                languageDisplayNames.Add(nativeName);
                            }
                            else
                            {
                                languageDisplayNames.Add(code.ToUpper());
                            }
                        }
                        else
                        {
                            languageDisplayNames.Add(code.ToUpper()); // Fallback if file doesn't exist yet
                        }
                    }
                }
            }

            Debug.Log($"[LocalizationManager] Manifest parsed. Detected {languageCodes.Count} available languages.");
        }

        /// <summary>
        /// Load language file dynamically from any local project Resources path.
        /// </summary>
        public void LoadLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogError("[LocalizationManager] LoadLanguage called with empty languageCode");
                return;
            }

            string fullResourcePath = $"{resourcesFolder}/{languageCode}";
            TextAsset txt = Resources.Load<TextAsset>(fullResourcePath);

            if (txt == null)
            {
                Debug.LogWarning($"[LocalizationManager] Localization file not found at: 'Resources/{fullResourcePath}.json'. Please make sure your sample assets are imported.");
                localizedText = new Dictionary<string, string>(); // Clean fallback state to prevent null reference errors
                CurrentLanguageCode = languageCode;
                OnLanguageChanged?.Invoke();
                return;
            }

            localizedText = ParseFlatJsonToDictionary(txt.text);
            CurrentLanguageCode = languageCode;
            Debug.Log($"[LocalizationManager] Loaded language '{languageCode}' with {localizedText.Count} entries.");

            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Change language using dropdown index.
        /// </summary>
        public void SetLanguageIndex(int index)
        {
            if (index < 0 || index >= languageCodes.Count) return;

            PlayerPrefs.SetInt(prefsKey, index);
            PlayerPrefs.Save();
            LoadLanguage(languageCodes[index]);
        }

        public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.TryGetValue(key, out string value))
            return value;
        return $"[MISSING:{key}]";
    }

    private Dictionary<string, string> ParseFlatJsonToDictionary(string json)
    {
        var dict = new Dictionary<string, string>();
        var raw = Json.Deserialize(json) as Dictionary<string, object>;
        if (raw != null)
        {
            foreach (var kvp in raw)
                dict[kvp.Key] = kvp.Value.ToString();
        }
        return dict;
    }
}
}
