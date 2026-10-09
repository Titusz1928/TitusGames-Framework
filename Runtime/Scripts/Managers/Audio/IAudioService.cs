using UnityEngine;

namespace TitusGames.Framework
{
    public interface IAudioService
    {
        // --- SFX (Direct AudioClip Overloads) ---
        void PlaySFX(AudioClip clip, float volumeMultiplier = 1f);
        void PlayRandomizedSFX(AudioClip clip, float pitchRange = 0.1f, float volumeRange = 0.1f);
        void PlayRandomSFXFromList(AudioClip[] clips, float pitchRange = 0.1f, float volumeRange = 0.1f);

        // --- SFX (Resources String Overloads) ---
        void PlaySFX(string clipName, float volumeMultiplier = 1f);
        void PlayRandomizedSFX(string clipName, float pitchRange = 0.1f, float volumeRange = 0.1f);
        void PlayRandomSFXFromList(string[] clipNames, float pitchRange = 0.1f, float volumeRange = 0.1f);

        // --- Music ---
        void PlayMusic(AudioClip clip, bool fade = true);
        void PlayMusic(string clipName, bool fade = true);
        void ResumePreviousMusic(string previousTrack, bool fade = true);
        void StopMusic(bool fade = true);
        string GetCurrentTrackName();

        // --- Volume & Toggles ---
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void ToggleMusic(bool isOn);
        void ToggleSFX(bool isOn);

        float GetMusicVolume();
        float GetSFXVolume();
        bool IsMusicOn();
        bool IsSFXOn();

        // --- Utility ---
        AudioClip GetClip(string clipName);
    }
}