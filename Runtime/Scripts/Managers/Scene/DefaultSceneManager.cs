using UnityEngine;

namespace TitusGames.Framework
{
    public class DefaultSceneManager : BaseSceneManager
    {
        protected override void OnCleanupSession()
        {
            // Default package version has no game-specific objects to destroy.
        }
    }
}