using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For UI components if needed directly, though we mostly use dynamic events

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")]
    public string gameSceneName = "Game"; // Trang gameplay
    public GameObject menuPanel;
    public GameObject settingsPanel;

    [Header("Setting Keys")]
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string QUALITY_INDEX_KEY = "QualityIndex";
    private const string FULLSCREEN_KEY = "Fullscreen";

    private void Awake()
    {
        // Fail-safe: Auto-create camera if missing to prevent "No Cameras rendering" error
        if (Camera.main == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f); // Dark gray background
            
            // Ensure AudioListener exists too for sound
            cameraObj.AddComponent<AudioListener>();
            
            Debug.Log("MainMenuController Warning: No Main Camera found. Created a fallback camera automatically.");
        }

        // INIT UI STATE HERE (Moved from Start to prevent 1-frame glitches)
        Debug.Log($"MainMenuController: Initializing UI. MenuPanel: {menuPanel}, SettingsPanel: {settingsPanel}");
        
        if (settingsPanel != null) 
        {
            settingsPanel.SetActive(false);
            Debug.Log("MainMenuController: Disabled SettingsPanel.");
        }
        else
        {
            Debug.LogError("MainMenuController: SettingsPanel reference is MISSING!");
        }

        if (menuPanel != null) 
        {
            menuPanel.SetActive(true);
            Debug.Log("MainMenuController: Enabled MenuPanel.");
        }
    }

    private void Start()
    {
        LoadSettings();
    }

    public void StartGame() // Bắt đầu game
    {
        Debug.Log("MainMenuController: StartGame called.");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings() // Mở trang settings
    {
        Debug.Log("MainMenuController: OpenSettings called.");
        if (settingsPanel != null) 
        {
            settingsPanel.SetActive(true);
            Debug.Log("MainMenuController: Settings Panel ENABLED.");
        }
        else
        {
            Debug.LogError("MainMenuController: Cannot open Settings - settingsPanel is null.");
        }
    }

    public void CloseSettings() // Đóng trang settings
    {
        Debug.Log("MainMenuController: CloseSettings called.");
        if (settingsPanel != null) 
        {
            settingsPanel.SetActive(false);
            Debug.Log("MainMenuController: Settings Panel DISABLED.");
        }
        else
        {
            Debug.LogError("MainMenuController: Cannot close Settings - settingsPanel is null.");
        }
        SaveSettings(); // Optional: Save when closing settings
    }

    public void QuitGame() // Thoát khỏi game
    {
        Debug.Log("MainMenuController: QuitGame called. Application.Quit() triggered.");
        Application.Quit();
    }

    #region Settings Logic

    // --- Audio ---
    // Note: In a real production app, you might route these to an AudioMixer
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
    }

    public void SetMusicVolume(float volume)
    {
        // Placeholder for specific Music AudioSource or Mixer Group
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        // Example: AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        // Placeholder for specific SFX AudioSource or Mixer Group
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    // --- Graphics ---
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QUALITY_INDEX_KEY, qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    // --- Persistence ---
    private void LoadSettings()
    {
        // Load Volume
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f);
        AudioListener.volume = masterVol;
        // Logic to update UI Sliders would go here if we had references to them

        // Load Quality
        int qualityIndex = PlayerPrefs.GetInt(QUALITY_INDEX_KEY, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(qualityIndex);

        // Load Fullscreen
        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFullscreen;
    }

    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    #endregion
}