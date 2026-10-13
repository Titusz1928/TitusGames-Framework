using UnityEngine;
using UnityEngine.SceneManagement;

namespace TitusGames.Framework
{
    public abstract class BaseSceneManager : MonoBehaviour, ISceneService
    {
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Current.Register<ISceneService>(this, overwrite: true);
        }

        public virtual void LoadScene(string sceneName)
        {
            if (sceneName == "MainMenu" || sceneName.Contains("Menu"))
            {
                OnCleanupSession();
            }
            SceneManager.LoadScene(sceneName);
        }

        public virtual void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Subclasses implement this to clean up game-specific systems
        protected abstract void OnCleanupSession();
    }
}