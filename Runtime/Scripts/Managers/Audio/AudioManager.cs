using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace TitusGames.Framework
{

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Resource Folder Paths")]
    [Tooltip("Path relative to any Resources folder where your music tracks are located.")]
    public string musicFolderPath = "Audio/Music";

    [Tooltip("Path relative to any Resources folder where your sound effects are located.")]
    public string sfxFolderPath = "Audio/SFX";

    [Header("Pool Settings")]
    [SerializeField] private int sfxSourcePoolSize = 4;

    private AudioSource musicSource;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int currentPoolIndex = 0;

    private Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMusicOn = true;
    private bool isSFXOn = true;

    private string currentTrackName;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        for (int i = 0; i < sfxSourcePoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D 
            source.ignoreListenerPause = true;
            sfxPool.Add(source);
        }

        LoadSettings();
    }

        private AudioClip GetOrCreateAudioClip(string clipName, bool isMusic)
        {
            var library = isMusic ? musicLibrary : sfxLibrary;

            // Check if we already loaded it previously
            if (library.TryGetValue(clipName, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            // Lazy load dynamically from Resources using the configurable path slug
            string targetPath = isMusic ? $"{musicFolderPath}/{clipName}" : $"{sfxFolderPath}/{clipName}";
            AudioClip newlyLoadedClip = Resources.Load<AudioClip>(targetPath);

            if (newlyLoadedClip != null)
            {
                library[clipName] = newlyLoadedClip;
                return newlyLoadedClip;
            }

            Debug.LogWarning($"[AudioManager] Audio asset '{clipName}' could not be located at 'Resources/{targetPath}'. Please verify file placement.");
            return null;
        }

        private AudioSource GetNextSFXSource()
    {
        AudioSource source = sfxPool[currentPoolIndex];
        currentPoolIndex = (currentPoolIndex + 1) % sfxPool.Count;
        return source;
    }

        // --- GENERIC PUBLIC METHODS ---

        public void PlaySFX(string clipName, float volumeMultiplier = 1f)
        {
            if (!isSFXOn || string.IsNullOrEmpty(clipName)) return;

            // Route through the lazy-loader helper
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: false);

            if (clip != null)
            {
                AudioSource source = GetNextSFXSource();
                source.pitch = 1f;
                source.PlayOneShot(clip, sfxVolume * volumeMultiplier);
            }
        }

        //NEW
        public void PlayRandomizedSFX(string clipName, float pitchRange = 0.1f, float volumeRange = 0.1f)
        {
            if (!isSFXOn || string.IsNullOrEmpty(clipName)) return;

            // Route through the lazy-loader helper
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: false);

            if (clip != null)
            {
                AudioSource source = GetNextSFXSource();
                source.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
                float randomVolume = Random.Range(sfxVolume - volumeRange, sfxVolume);
                source.PlayOneShot(clip, Mathf.Clamp01(randomVolume));
            }
        }

    public void PlayRandomSFXFromList(string[] clipNames, float pitchRange = 0.1f, float volumeRange = 0.1f)
    {
        if (clipNames == null || clipNames.Length == 0) return;

        int randomIndex = Random.Range(0, clipNames.Length);
        PlayRandomizedSFX(clipNames[randomIndex], pitchRange, volumeRange);
    }

        public void PlayMusic(string clipName, bool fade = true)
        {
            if (string.IsNullOrEmpty(clipName)) return;

            // Use your lazy-loader helper instead of reading the raw dictionary!
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: true);

            if (clip != null)
            {
                if (musicSource.clip == clip) return; // Already playing

                currentTrackName = clipName;
                if (fade) StartCoroutine(FadeToNewTrack(clip));
                else SwitchMusicInstant(clip);
            }
        }

        public void ResumePreviousMusic(string previousTrack, bool fade = true)
    {
        if (!string.IsNullOrEmpty(previousTrack)) PlayMusic(previousTrack, fade);
    }

    public string GetCurrentTrackName() => currentTrackName;

    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            // Start an empty fade (passing null clip)
            StartCoroutine(FadeToNewTrack(null));
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private void SwitchMusicInstant(AudioClip clip)
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.mute = !isMusicOn;
        if (clip != null) musicSource.Play();
    }

    private System.Collections.IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float fadeDuration = 0.8f;

        if (musicSource.isPlaying)
        {
            // Use a local timer with unscaledDeltaTime
            for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(musicVolume, 0f, t / fadeDuration);
                yield return null; // This will now continue even if Time.timeScale is 0
            }
        }

        SwitchMusicInstant(newClip);

        if (newClip != null)
        {
            for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeDuration);
                yield return null;
            }
            musicSource.volume = musicVolume;
        }
    }

    // --- Volume & Toggle Management ---
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (var source in sfxPool) source.volume = sfxVolume;
        SaveSettings();
    }

    public void ToggleMusic(bool isOn)
    {
        isMusicOn = isOn;
        musicSource.mute = !isOn;
        SaveSettings();
    }

    public void ToggleSFX(bool isOn)
    {
        isSFXOn = isOn;
        foreach (var source in sfxPool) source.mute = !isOn;
        SaveSettings();
    }

    // --- Getters for UI Sync ---
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicOn() => isMusicOn;
    public bool IsSFXOn() => isSFXOn;

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("IsSFXOn", 1) == 1;

        musicSource.volume = musicVolume;
        musicSource.mute = !isMusicOn;

        foreach (var source in sfxPool)
        {
            source.volume = sfxVolume;
            source.mute = !isSFXOn;
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("IsSFXOn", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    //NEW
    public AudioClip GetClip(string clipName)
    {
            return GetOrCreateAudioClip(clipName, isMusic: false);
        }

}
}
