using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TitusGames.Framework
{
public class LanguageManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;


    private string PREF_KEY = "languageIndex"; // Matched naming with LocalizationManager

        void Start()
        {
            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("[LanguageManager] Cannot initialize: LocalizationManager Instance missing from scene context.");
                return;
            }

            // Read the total available configurations directly from the initialized manager lists
            int savedLang = PlayerPrefs.GetInt(PREF_KEY, 0);
            savedLang = Mathf.Clamp(savedLang, 0, Mathf.Max(0, LocalizationManager.Instance.languageCodes.Count - 1));

            PopulateDropdown();

            if (dropdown.options.Count > 0)
            {
                dropdown.value = savedLang;
            }

            dropdown.onValueChanged.AddListener(OnLanguageChanged);
            UpdateDropdownLabels();
        }

        private void PopulateDropdown()
        {
            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            // Generate temporary placeholder options based on length
            int count = LocalizationManager.Instance.languageCodes.Count;
            for (int i = 0; i < count; i++)
            {
                options.Add(new TMP_Dropdown.OptionData(""));
            }

            dropdown.AddOptions(options);
            UpdateDropdownLabels(); // Pull correct translations instantly
        }

        private void UpdateDropdownLabels()
        {
            var codes = LocalizationManager.Instance.languageCodes;

            for (int i = 0; i < codes.Count; i++)
            {
                if (i < dropdown.options.Count)
                {
                    // Live Localized Value Search: Ask the system how to say the language code inside the current language context!
                    string liveLocalizedName = LocalizationManager.Instance.GetLocalizedValue(codes[i]);

                    // If the active translation dictionary doesn't have it, fallback to its code
                    if (liveLocalizedName.StartsWith("[MISSING:"))
                    {
                        dropdown.options[i].text = codes[i].ToUpper();
                    }
                    else
                    {
                        dropdown.options[i].text = liveLocalizedName;
                    }
                }
            }
            dropdown.RefreshShownValue();
        }

        private void OnLanguageChanged(int index)
        {
            ApplyLanguage(index);
        }

        private void ApplyLanguage(int index)
        {
            if (index < 0 || index >= LocalizationManager.Instance.languageCodes.Count) return;

            Debug.Log($"[LanguageManager] UI switching system context to code: {LocalizationManager.Instance.languageCodes[index]}");

            LocalizationManager.Instance.SetLanguageIndex(index);
            UpdateDropdownLabels();

            /*if (MessageManager.Instance != null)
                MessageManager.Instance.ShowMessage("testmessage");*/
        }

        /// <summary>
        /// Generalized helper execution shortcut for custom UI button hooks.
        /// Pass the explicit integer element ID directly through Unity's Event Trigger inspector panel.
        /// </summary>
        public void SelectLanguageViaIndex(int targetIndex)
        {
            if (dropdown != null)
            {
                dropdown.value = targetIndex; // This automatically executes OnLanguageChanged via listener triggers
            }
            else
            {
                ApplyLanguage(targetIndex);
            }
        }
    }
}