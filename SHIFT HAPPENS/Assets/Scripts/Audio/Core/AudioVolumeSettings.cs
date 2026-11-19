using System;
using UnityEngine;

[Serializable]
public class AudioVolumeSettings
{
    private const string Prefix = "ShiftHappens.Audio.";
    private const string SavedKey = Prefix + "Saved";
    private const string MasterKey = Prefix + "Master";
    private const string MusicOnKey = Prefix + "MusicOn";
    private const string MusicKey = Prefix + "Music";
    private const string SfxOnKey = Prefix + "SfxOn";
    private const string SfxKey = Prefix + "Sfx";
    private const string UiOnKey = Prefix + "UiOn";
    private const string UiKey = Prefix + "Ui";

    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    [Space]
    [SerializeField] private bool musicEnabled = true;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    [Space]
    [SerializeField] private bool sfxEnabled = true;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Space]
    [SerializeField] private bool uiEnabled = true;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 0.8f;

    public float MasterVolume { get => masterVolume; set => masterVolume = Mathf.Clamp01(value); }
    public bool MusicEnabled { get => musicEnabled; set => musicEnabled = value; }
    public float MusicVolume { get => musicVolume; set => musicVolume = Mathf.Clamp01(value); }
    public bool SfxEnabled { get => sfxEnabled; set => sfxEnabled = value; }
    public float SfxVolume { get => sfxVolume; set => sfxVolume = Mathf.Clamp01(value); }
    public bool UiEnabled { get => uiEnabled; set => uiEnabled = value; }
    public float UiVolume { get => uiVolume; set => uiVolume = Mathf.Clamp01(value); }

    public bool IsEnabled(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Music: return musicEnabled;
            case AudioBus.UI: return uiEnabled;
            default: return sfxEnabled;
        }
    }

    public float GetBusVolume(AudioBus bus)
    {
        if (!IsEnabled(bus)) return 0f;
        switch (bus)
        {
            case AudioBus.Music: return masterVolume * musicVolume;
            case AudioBus.UI: return masterVolume * uiVolume;
            default: return masterVolume * sfxVolume;
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(SavedKey, 1);
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        PlayerPrefs.SetInt(MusicOnKey, musicEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.SetInt(SfxOnKey, sfxEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.SetInt(UiOnKey, uiEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(UiKey, uiVolume);
        PlayerPrefs.Save();
    }

    public bool Load()
    {
        if (!PlayerPrefs.HasKey(SavedKey)) return false;
        masterVolume = PlayerPrefs.GetFloat(MasterKey, masterVolume);
        musicEnabled = PlayerPrefs.GetInt(MusicOnKey, musicEnabled ? 1 : 0) == 1;
        musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
        sfxEnabled = PlayerPrefs.GetInt(SfxOnKey, sfxEnabled ? 1 : 0) == 1;
        sfxVolume = PlayerPrefs.GetFloat(SfxKey, sfxVolume);
        uiEnabled = PlayerPrefs.GetInt(UiOnKey, uiEnabled ? 1 : 0) == 1;
        uiVolume = PlayerPrefs.GetFloat(UiKey, uiVolume);
        return true;
    }

    public static void ClearSaved()
    {
        PlayerPrefs.DeleteKey(SavedKey);
        PlayerPrefs.DeleteKey(MasterKey);
        PlayerPrefs.DeleteKey(MusicOnKey);
        PlayerPrefs.DeleteKey(MusicKey);
        PlayerPrefs.DeleteKey(SfxOnKey);
        PlayerPrefs.DeleteKey(SfxKey);
        PlayerPrefs.DeleteKey(UiOnKey);
        PlayerPrefs.DeleteKey(UiKey);
        PlayerPrefs.Save();
    }
}
