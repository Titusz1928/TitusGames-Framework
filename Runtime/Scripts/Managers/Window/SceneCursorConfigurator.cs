using UnityEngine;

namespace TitusGames.Framework
{
    public class SceneCursorConfigurator : MonoBehaviour
    {
        [Header("Cursor Configuration")]
        [Tooltip("If true, locks and hides mouse when no windows are open (3D gameplay). If false, keeps cursor free.")]
        [SerializeField] private bool lockCursorForScene = true;

        [Header("Transition Settings")]
        [Tooltip("If true, automatically resets the cursor lock to false when this scene unloads. Turn this OFF if you are transitioning to another gameplay scene that also needs a locked cursor.")]
        [SerializeField] private bool revertOnDestroy = true;

        private void Start()
        {
            ApplyConfig();
        }

        private void ApplyConfig()
        {
            // Fetch the persistent WindowManager from the ServiceLocator
            if (ServiceLocator.Current != null && ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
            {
                if (windowService is WindowManager wm)
                {
                    wm.SetGameplayCursorLock(lockCursorForScene);
                }
            }
        }

        private void OnDestroy()
        {
            if (!revertOnDestroy) return;

            // Revert back to menu/default style (false) when leaving this scene
            if (ServiceLocator.Current != null && ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
            {
                if (windowService is WindowManager wm)
                {
                    wm.SetGameplayCursorLock(false);
                }
            }
        }
    }
}