using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TitusGames.Framework
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [Header("Resource Folder Paths")]
        public string musicFolderPath = "Audio/Music";
        public string sfxFolderPath = "Audio/SFX";

        [Header("Pool Settings")]
        [SerializeField] private int sfxSourcePoolSize = 6;

        private AudioSource musicSource;
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentPoolIndex = 0;

        private readonly Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private bool isMusicOn = true;
        private bool isSFXOn = true;
        private string currentTrackName;

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;

            for (int i = 0; i < sfxSourcePoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.spatialBlend = 0f;
                source.ignoreListenerPause = true;
                sfxPool.Add(source);
            }

            LoadSettings();
        }

        // --- SFX PUBLIC METHODS (AudioClip Overloads) ---

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (!isSFXOn || clip == null) return;

            AudioSource source = GetNextSFXSource();
            source.pitch = 1f;
            source.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        public void PlayRandomizedSFX(AudioClip clip, float pitchRange = 0.1f, float volumeRange = 0.1f)
        {
            if (!isSFXOn || clip == null) return;

            AudioSource source = GetNextSFXSource();
            source.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
            float randomVolume = Random.Range(sfxVolume - volumeRange, sfxVolume);
            source.PlayOneShot(clip, Mathf.Clamp01(randomVolume));
        }

        public void PlayRandomSFXFromList(AudioClip[] clips, float pitchRange = 0.1f, float volumeRange = 0.1f)
        {
            if (clips == null || clips.Length == 0) return;

            int randomIndex = Random.Range(0, clips.Length);
            PlayRandomizedSFX(clips[randomIndex], pitchRange, volumeRange);
        }

        // --- SFX PUBLIC METHODS (String Overloads) ---

        public void PlaySFX(string clipName, float volumeMultiplier = 1f)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: false);
            PlaySFX(clip, volumeMultiplier);
        }

        public void PlayRandomizedSFX(string clipName, float pitchRange = 0.1f, float volumeRange = 0.1f)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: false);
            PlayRandomizedSFX(clip, pitchRange, volumeRange);
        }

        public void PlayRandomSFXFromList(string[] clipNames, float pitchRange = 0.1f, float volumeRange = 0.1f)
        {
            if (clipNames == null || clipNames.Length == 0) return;

            int randomIndex = Random.Range(0, clipNames.Length);
            PlayRandomizedSFX(clipNames[randomIndex], pitchRange, volumeRange);
        }

        // --- MUSIC METHODS ---

        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return; // Already playing

            currentTrackName = clip.name;
            if (fade) StartCoroutine(FadeToNewTrack(clip));
            else SwitchMusicInstant(clip);
        }

        public void PlayMusic(string clipName, bool fade = true)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            AudioClip clip = GetOrCreateAudioClip(clipName, isMusic: true);
            PlayMusic(clip, fade);
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

        private IEnumerator FadeToNewTrack(AudioClip newClip)
        {
            float fadeDuration = 0.8f;

            if (musicSource.isPlaying)
            {
                for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
                {
                    musicSource.volume = Mathf.Lerp(musicVolume, 0f, t / fadeDuration);
                    yield return null;
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

        // --- UTILITY & CACHING ---

        public AudioClip GetClip(string clipName)
        {
            return GetOrCreateAudioClip(clipName, isMusic: false);
        }

        private AudioClip GetOrCreateAudioClip(string clipName, bool isMusic)
        {
            var library = isMusic ? musicLibrary : sfxLibrary;

            if (library.TryGetValue(clipName, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            string targetPath = isMusic ? $"{musicFolderPath}/{clipName}" : $"{sfxFolderPath}/{clipName}";
            AudioClip newlyLoadedClip = Resources.Load<AudioClip>(targetPath);

            if (newlyLoadedClip != null)
            {
                library[clipName] = newlyLoadedClip;
                return newlyLoadedClip;
            }

            Debug.LogWarning($"[AudioManager] Audio asset '{clipName}' could not be located at 'Resources/{targetPath}'.");
            return null;
        }

        private AudioSource GetNextSFXSource()
        {
            AudioSource source = sfxPool[currentPoolIndex];
            currentPoolIndex = (currentPoolIndex + 1) % sfxPool.Count;
            return source;
        }

        // --- VOLUME & SETTINGS ---

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
    }
}