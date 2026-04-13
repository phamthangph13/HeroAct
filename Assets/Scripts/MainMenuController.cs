using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")]
    public string gameSceneName = "Hub";
    public GameObject menuPanel;
    public GameObject settingsPanel;

    private void Awake()
    {
        if (Camera.main == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

            if (Object.FindFirstObjectByType<AudioListener>() == null)
            {
                cameraObj.AddComponent<AudioListener>();
            }

            Debug.Log("MainMenuController: Created fallback camera.");
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("MainMenuController: Settings panel reference is missing.");
        }

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }

    private void Start()
    {
        GameSettings.ApplyAll();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuController: Cannot open settings because the panel is null.");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("MainMenuController: Cannot close settings because the panel is null.");
        }

        GameSettings.Save();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetMasterVolume(float volume)
    {
        GameSettings.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        GameSettings.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        GameSettings.SetSfxVolume(volume);
    }

    public void SetQuality(int qualityIndex)
    {
        GameSettings.SetQualityIndex(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        GameSettings.SetFullscreen(isFullscreen);
    }

    public void SetTargetFrameRate(int targetFrameRate)
    {
        GameSettings.SetTargetFrameRate(targetFrameRate);
    }
}
