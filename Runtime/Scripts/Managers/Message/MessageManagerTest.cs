using System.Collections;
using UnityEngine;

namespace TitusGames.Framework
{
    public class MessageManagerTester : MonoBehaviour
    {
        [Header("Testing Loop Configuration")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float interval = 5f;

        [Header("Explicit Sprite Test Injection")]
        [Tooltip("Assign a test sprite here to check the direct object injection path (Scenario 4)")]
        [SerializeField] private Sprite runtimeCustomSprite;

        private readonly string[] customPrefabs = { "BottomMessagePrefab", "WhiteMessagePrefab" };
        private readonly string[] customIcons = { "info", "info2" };

        private readonly string[] samplePhrases = {
            "Default test message simulation.",
            "Warning: Custom icon layout!",
            "Successful custom template variant configuration.",
            "Fully overridden style settings applied.",
            "Direct Sprite memory injection verified."
        };

        private Coroutine testRoutine;

        private void Start()
        {
            if (runOnStart)
            {
                StartTestingLoop();
            }
        }

        public void StartTestingLoop()
        {
            if (testRoutine == null)
            {
                testRoutine = StartCoroutine(PeriodicMessageGenerator());
                Debug.Log("[MessageTester] API fallback sequence loop initialized.");
            }
        }

        public void StopTestingLoop()
        {
            if (testRoutine != null)
            {
                StopCoroutine(testRoutine);
                testRoutine = null;
                Debug.Log("[MessageTester] Testing loop suspended.");
            }
        }

        private IEnumerator PeriodicMessageGenerator()
        {
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                TriggerRandomScenario();
                yield return new WaitForSeconds(interval);
            }
        }

        [ContextMenu("Trigger Single Random Scenario")]
        public void TriggerRandomScenario()
        {
            if (MessageManager.Instance == null)
            {
                Debug.LogError("[MessageTester] MessageManager.Instance is missing from scene environment.");
                return;
            }

            // Pick a scenario index from 0 to 4
            int scenario = Random.Range(0, 5);
            string phrase = samplePhrases[Random.Range(0, samplePhrases.Length)];

            switch (scenario)
            {
                case 0:
                    // 1. ABSOLUTE DEFAULT: Test parameter omission.
                    // Both fields fallback entirely to Inspector configurations inside the manager.
                    Debug.Log($"[MessageTester] Scenario 0: Omitted parameters -> \"{phrase}\"");
                    MessageManager.Instance.ShowMessageDirectly(phrase);
                    break;

                case 1:
                    // 2. SPECIFYING ICON ONLY: Test default prefab fallback alongside custom data.
                    string targetIcon = customIcons[Random.Range(0, customIcons.Length)];
                    Debug.Log($"[MessageTester] Scenario 1: Default Prefab + Custom Icon ('{targetIcon}') -> \"{phrase}\"");
                    MessageManager.Instance.ShowMessageDirectly(phrase, iconName: targetIcon);
                    break;

                case 2:
                    // 3. SPECIFYING PREFAB ONLY: Leave icon string empty, should load default icon inside custom layout.
                    string targetPrefab = customPrefabs[Random.Range(0, customPrefabs.Length)];
                    Debug.Log($"[MessageTester] Scenario 2: Custom Prefab ('{targetPrefab}') + Default Icon -> \"{phrase}\"");
                    MessageManager.Instance.ShowMessageDirectly(phrase, customPrefabName: targetPrefab);
                    break;

                case 3:
                    // 4. FULL OVERRIDE: Both fields explicitly filled with custom project strings.
                    string overIcon = customIcons[Random.Range(0, customIcons.Length)];
                    string overPrefab = customPrefabs[Random.Range(0, customPrefabs.Length)];
                    Debug.Log($"[MessageTester] Scenario 3: Complete Parameter Override (Prefab: '{overPrefab}', Icon: '{overIcon}') -> \"{phrase}\"");
                    MessageManager.Instance.ShowMessageDirectly(phrase, overIcon, overPrefab);
                    break;

                case 4:
                    // 5. DIRECT SPRITE OVERLOAD: Test the ShowMessageWithSprite alternate pipeline pathway.
                    string spritePrefab = customPrefabs[Random.Range(0, customPrefabs.Length)];
                    if (runtimeCustomSprite == null)
                    {
                        Debug.LogWarning("[MessageTester] Scenario 4 skipped because 'runtimeCustomSprite' field is unassigned in the inspector. Running Scenario 0 fallback instead.");
                        MessageManager.Instance.ShowMessageDirectly(phrase);
                    }
                    else
                    {
                        Debug.Log($"[MessageTester] Scenario 4: Direct Sprite Injection (Prefab: '{spritePrefab}') -> \"{phrase}\"");
                        MessageManager.Instance.ShowMessageWithSprite(phrase, runtimeCustomSprite, customPrefabName: spritePrefab, useLocalization: false);
                    }
                    break;
            }
        }
    }
}