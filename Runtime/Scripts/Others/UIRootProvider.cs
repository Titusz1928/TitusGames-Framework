using UnityEngine;

namespace TitusGames.Framework
{
    public class UIRootProvider : MonoBehaviour
    {
        public Transform windowRoot;

        private void Start()
        {
            WindowManager.Instance.RegisterUIRoot(windowRoot);
        }
    }
}
