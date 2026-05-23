using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TitusGames.Framework
{
public class LanguageManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;

    // Must match the order in LocalizationManager
    private string[] languageCodes = { "eng", "hun", "rom" }; 
    private string PREF_KEY = "languageIndex"; // Matched naming with LocalizationManager

    void Start()
    {
        // Load saved language or default to 0 (English)
        int savedLang = PlayerPrefs.GetInt(PREF_KEY, 0);
        savedLang = Mathf.Clamp(savedLang, 0, languageCodes.Length - 1);

        PopulateDropdown();
        dropdown.value = savedLang;
        
        // Listen to dropdown changes
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
        
        // Initial label sync
        UpdateDropdownLabels();
    }

    private void PopulateDropdown()
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        
        for (int i = 0; i < languageCodes.Length; i++)
        {
            // Gets the localized name of the language itself (e.g., "English", "Română")
            string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
            options.Add(new TMP_Dropdown.OptionData(localizedName));
        }

        dropdown.AddOptions(options);
    }

    private void UpdateDropdownLabels()
    {
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (i < dropdown.options.Count)
                dropdown.options[i].text = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
        }
        dropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int index)
    {
        ApplyLanguage(index);
    }

    private void ApplyLanguage(int index)
    {
        Debug.Log($"Switching language to: {languageCodes[index]}");

        // Update the actual localization system
        LocalizationManager.Instance.SetLanguageIndex(index);
        
        // Visual feedback
        UpdateDropdownLabels();

        // Test message example
        if (MessageManager.Instance != null)
            MessageManager.Instance.ShowMessage("testmessage");
    }

    // Helper methods for specific buttons
    public void SelectEnglish() => OnLanguageChanged(0);
    public void SelectHungarian() => OnLanguageChanged(1);
    public void SelectRomanian() => OnLanguageChanged(2);
}
}