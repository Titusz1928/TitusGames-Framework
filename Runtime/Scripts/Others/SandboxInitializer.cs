using UnityEngine;

namespace TitusGames.Framework
{
    public class SandboxInitializer : MonoBehaviour
    {
        private void Awake()
        {
            InitializeGlobalManagers();
        }

        private void InitializeGlobalManagers()
        {
            // Initialize infrastructure registry safely without wiping pre-registered project overrides
            ServiceLocator.Initialize(clearExisting: false);

            // 1. Scene Service (Will respect custom project managers if already registered in the scene)
            ServiceLocator.Current.GetOrRegister<ISceneService>(() =>
            {
                var sceneObj = new GameObject("DefaultSceneManager");
                DontDestroyOnLoad(sceneObj);
                return sceneObj.AddComponent<DefaultSceneManager>();
            });

            // 2. Window Service
            ServiceLocator.Current.GetOrRegister<IWindowService>(() =>
            {
                var windowObj = new GameObject("WindowManager");
                DontDestroyOnLoad(windowObj);
                return windowObj.AddComponent<WindowManager>();
            });

            // 3. Localization Service
            var localizationService = ServiceLocator.Current.GetOrRegister<ILocalizationService>(() =>
            {
                var locObj = new GameObject("LocalizationManager");
                DontDestroyOnLoad(locObj);
                return locObj.AddComponent<LocalizationManager>();
            });
            localizationService?.Initialize();

            // 4. Audio Service
            ServiceLocator.Current.GetOrRegister<IAudioService>(() =>
            {
                var audioObj = new GameObject("AudioManager");
                DontDestroyOnLoad(audioObj);
                return audioObj.AddComponent<AudioManager>();
            });

            // 5. Message Service
            ServiceLocator.Current.GetOrRegister<IMessageService>(() =>
            {
                var msgObj = new GameObject("MessageManager");
                DontDestroyOnLoad(msgObj);
                var service = msgObj.AddComponent<MessageManager>();

                var container = GameObject.Find("MessageContainer");
                if (container != null && container.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    service.RegisterContainer(rectTransform);
                }

                return service;
            });

            Debug.Log("<color=cyan>[Sandbox] Global Infrastructure & Services Initialized Cleanly via ServiceLocator.</color>");
        }
    }
}