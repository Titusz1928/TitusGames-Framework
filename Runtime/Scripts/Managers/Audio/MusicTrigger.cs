using UnityEngine;

namespace TitusGames.Framework
{
public class MusicTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string trackName;
    [SerializeField] private bool fade = true;

    [Header("Scene Transition")]
    [Tooltip("If true, the music will stop when this scene is unloaded.")]
    [SerializeField] private bool stopOnSceneExit = false;

    void Start()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(trackName))
        {
            AudioManager.Instance.PlayMusic(trackName, fade);
        }
    }

    private void OnDestroy()
    {
        // When the scene changes, this object is destroyed.
        // We check the boolean to decide if we should silence the music.
        if (stopOnSceneExit && AudioManager.Instance != null)
        {
            // We can pass an empty string or null to a new StopMusic method 
            // or just tell the manager to stop the current source.
            AudioManager.Instance.StopMusic(fade);
        }
    }
}
}