using UnityEngine;
using UnityEngine.InputSystem;

namespace TitusGames.Framework
{
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("TitusGames/Framework/Input Linker")]
    public class InputLinker : MonoBehaviour
    {
        private PlayerInput playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            if (WindowManager.Instance != null)
            {
                WindowManager.Instance.RegisterPlayerInput(playerInput);
            }
        }

        private void OnDestroy()
        {
            if (WindowManager.Instance != null)
            {
                // Cleanly unregister this input context if destroyed/scene changes
                WindowManager.Instance.RegisterPlayerInput(null);
            }
        }
    }
}