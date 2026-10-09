using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TitusGames.Framework
{
    public class Boot : MonoBehaviour
    {
        [Header("UI References")]
        public CanvasGroup bootCanvas; // assign your boot canvas in inspector

        [Header("Fade Settings")]
        public float fadeDuration = 1f;

        private IEnumerator Start()
        {
            // Make sure canvas is visible at start
            bootCanvas.alpha = 0f;
            bootCanvas.gameObject.SetActive(true);

            // Fade in
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

            // === Initialize the Service Locator infrastructure ===
            ServiceLocator.Initialize();

            RegisterCoreServices();

            // Optional small delay while visible
            yield return new WaitForSeconds(0.5f);

            // Fade out
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

            // Fetch the scene service from the locator to load the main menu
            ServiceLocator.Current.Get<ISceneService>().LoadScene("MainMenu");
        }

        private void RegisterCoreServices()
        {
            // 1. Window Service
            ServiceLocator.Current.GetOrRegister<IWindowService>(() =>
            {
                var go = new GameObject("WindowManager");
                DontDestroyOnLoad(go);
                return go.AddComponent<WindowManager>();
            });

            // 2. Localization Service
            var localizationService = ServiceLocator.Current.GetOrRegister<ILocalizationService>(() =>
            {
                var go = new GameObject("LocalizationManager");
                DontDestroyOnLoad(go);
                return go.AddComponent<LocalizationManager>();
            });
            localizationService?.Initialize();

            // 3. Message Service
            ServiceLocator.Current.GetOrRegister<IMessageService>(() =>
            {
                var go = new GameObject("MessageManager");
                DontDestroyOnLoad(go);
                var service = go.AddComponent<MessageManager>();

                var container = GameObject.Find("MessageContainer");
                if (container != null && container.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    service.RegisterContainer(rectTransform);
                }

                return service;
            });

            // 4. Scene Service
            ServiceLocator.Current.GetOrRegister<ISceneService>(() =>
            {
                var go = new GameObject("SceneManagerEX");
                DontDestroyOnLoad(go);
                return go.AddComponent<SceneManagerEX>();
            });

            // 5. Audio Service
            ServiceLocator.Current.GetOrRegister<IAudioService>(() =>
            {
                var go = new GameObject("AudioManager");
                DontDestroyOnLoad(go);
                return go.AddComponent<AudioManager>();
            });
        }

        private IEnumerator FadeCanvas(float start, float end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                bootCanvas.alpha = Mathf.Lerp(start, end, t);
                yield return null;
            }
            bootCanvas.alpha = end;
        }
    }
}