using UnityEngine;

public static class GameSettings
{
    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SFXVolume";
    public const string QualityIndexKey = "QualityIndex";
    public const string FullscreenKey = "Fullscreen";
    public const string TargetFrameRateKey = "TargetFrameRate";

    private const float DefaultMasterVolume = 1f;
    private const float DefaultMusicVolume = 0.5f;
    private const float DefaultSfxVolume = 1f;
    private const int DefaultTargetFrameRate = -1;

    private static bool defaultsCaptured;
    private static int bootQualityIndex;
    private static bool bootFullscreen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyBootSettings()
    {
        ApplyAll();
    }

    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
    }

    public static void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    public static float GetMusicVolume(float fallback = DefaultMusicVolume)
    {
        return PlayerPrefs.GetFloat(MusicVolumeKey, fallback);
    }

    public static void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        ApplyMusicVolume(volume);
    }

    public static float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);
    }

    public static void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
    }

    public static int GetQualityIndex()
    {
        EnsureBootDefaultsCaptured();
        return Mathf.Clamp(
            PlayerPrefs.GetInt(QualityIndexKey, bootQualityIndex),
            0,
            Mathf.Max(0, QualitySettings.names.Length - 1));
    }

    public static void SetQualityIndex(int qualityIndex)
    {
        if (QualitySettings.names.Length == 0)
        {
            return;
        }

        qualityIndex = Mathf.Clamp(qualityIndex, 0, QualitySettings.names.Length - 1);
        PlayerPrefs.SetInt(QualityIndexKey, qualityIndex);
        ApplyQuality(qualityIndex);
        ApplyFrameRate(GetTargetFrameRate());
    }

    public static bool GetFullscreen()
    {
        EnsureBootDefaultsCaptured();
        return PlayerPrefs.GetInt(FullscreenKey, bootFullscreen ? 1 : 0) == 1;
    }

    public static void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
    }

    public static int GetTargetFrameRate()
    {
        return PlayerPrefs.GetInt(TargetFrameRateKey, DefaultTargetFrameRate);
    }

    public static void SetTargetFrameRate(int targetFrameRate)
    {
        PlayerPrefs.SetInt(TargetFrameRateKey, targetFrameRate);
        ApplyQuality(GetQualityIndex());
        ApplyFrameRate(targetFrameRate);
    }

    public static void ApplyAll()
    {
        EnsureBootDefaultsCaptured();

        SetMasterVolume(GetMasterVolume());
        ApplyMusicVolume(GetMusicVolume());
        SetFullscreen(GetFullscreen());
        ApplyQuality(GetQualityIndex());
        ApplyFrameRate(GetTargetFrameRate());
    }

    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(MasterVolumeKey);
        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);
        PlayerPrefs.DeleteKey(QualityIndexKey);
        PlayerPrefs.DeleteKey(FullscreenKey);
        PlayerPrefs.DeleteKey(TargetFrameRateKey);
        ApplyAll();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static string FormatFrameRateLabel(int targetFrameRate)
    {
        return targetFrameRate > 0 ? $"{targetFrameRate} FPS" : "THEO THIET BI";
    }

    public static string FormatFullscreenLabel(bool isFullscreen)
    {
        return isFullscreen ? "BAT" : "TAT";
    }

    private static void EnsureBootDefaultsCaptured()
    {
        if (defaultsCaptured)
        {
            return;
        }

        bootQualityIndex = Mathf.Clamp(
            QualitySettings.GetQualityLevel(),
            0,
            Mathf.Max(0, QualitySettings.names.Length - 1));
        bootFullscreen = Screen.fullScreen;
        defaultsCaptured = true;
    }

    private static void ApplyQuality(int qualityIndex)
    {
        if (QualitySettings.names.Length == 0)
        {
            return;
        }

        QualitySettings.SetQualityLevel(qualityIndex, true);
    }

    private static void ApplyFrameRate(int targetFrameRate)
    {
        if (targetFrameRate > 0)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            return;
        }

        Application.targetFrameRate = -1;
    }

    private static void ApplyMusicVolume(float volume)
    {
        BackgroundMusic[] musicPlayers = Object.FindObjectsByType<BackgroundMusic>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (BackgroundMusic musicPlayer in musicPlayers)
        {
            musicPlayer.SetVolume(volume);
        }
    }
}
