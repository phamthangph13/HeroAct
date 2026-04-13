using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsModalController : MonoBehaviour
{
    private static readonly int[] FrameRateOptions = { -1, 30, 60, 90, 120 };

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeValueText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeValueText;

    [Header("Selectors")]
    public Button frameRatePrevButton;
    public Button frameRateNextButton;
    public TextMeshProUGUI frameRateValueText;
    public Button qualityPrevButton;
    public Button qualityNextButton;
    public TextMeshProUGUI qualityValueText;
    public Button fullscreenOffButton;
    public Button fullscreenOnButton;
    public TextMeshProUGUI fullscreenValueText;

    [Header("Actions")]
    public Button resetDefaultsButton;
    public TextMeshProUGUI saveHintText;

    private bool initialized;

    private void Awake()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(HandleMusicVolumeChanged);
        }

        if (frameRatePrevButton != null)
        {
            frameRatePrevButton.onClick.AddListener(() => StepFrameRate(-1));
        }

        if (frameRateNextButton != null)
        {
            frameRateNextButton.onClick.AddListener(() => StepFrameRate(1));
        }

        if (qualityPrevButton != null)
        {
            qualityPrevButton.onClick.AddListener(() => StepQuality(-1));
        }

        if (qualityNextButton != null)
        {
            qualityNextButton.onClick.AddListener(() => StepQuality(1));
        }

        if (fullscreenOffButton != null)
        {
            fullscreenOffButton.onClick.AddListener(() => SetFullscreen(false));
        }

        if (fullscreenOnButton != null)
        {
            fullscreenOnButton.onClick.AddListener(() => SetFullscreen(true));
        }

        if (resetDefaultsButton != null)
        {
            resetDefaultsButton.onClick.AddListener(ResetDefaults);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnDisable()
    {
        GameSettings.Save();
    }

    private void HandleMasterVolumeChanged(float value)
    {
        GameSettings.SetMasterVolume(value);
        UpdateVolumeLabel(masterVolumeValueText, value);
    }

    private void HandleMusicVolumeChanged(float value)
    {
        GameSettings.SetMusicVolume(value);
        UpdateVolumeLabel(musicVolumeValueText, value);
    }

    private void StepFrameRate(int direction)
    {
        int currentValue = GameSettings.GetTargetFrameRate();
        int currentIndex = Array.IndexOf(FrameRateOptions, currentValue);

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        int nextIndex = Mathf.Clamp(currentIndex + direction, 0, FrameRateOptions.Length - 1);
        GameSettings.SetTargetFrameRate(FrameRateOptions[nextIndex]);
        RefreshSelectorTexts();
    }

    private void StepQuality(int direction)
    {
        string[] qualityNames = QualitySettings.names;
        if (qualityNames == null || qualityNames.Length == 0)
        {
            return;
        }

        int nextIndex = Mathf.Clamp(GameSettings.GetQualityIndex() + direction, 0, qualityNames.Length - 1);
        GameSettings.SetQualityIndex(nextIndex);
        RefreshSelectorTexts();
    }

    private void SetFullscreen(bool isFullscreen)
    {
        GameSettings.SetFullscreen(isFullscreen);
        RefreshSelectorTexts();
    }

    private void ResetDefaults()
    {
        GameSettings.ResetToDefaults();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (masterVolumeSlider != null)
        {
            float masterVolume = GameSettings.GetMasterVolume();
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
            UpdateVolumeLabel(masterVolumeValueText, masterVolume);
        }

        if (musicVolumeSlider != null)
        {
            float musicVolume = GameSettings.GetMusicVolume();
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            UpdateVolumeLabel(musicVolumeValueText, musicVolume);
        }

        RefreshSelectorTexts();

        if (saveHintText != null)
        {
            saveHintText.text = "THAY DOI SE DUOC LUU KHI DONG MENU";
        }
    }

    private void RefreshSelectorTexts()
    {
        int frameRate = GameSettings.GetTargetFrameRate();
        if (frameRateValueText != null)
        {
            frameRateValueText.text = GameSettings.FormatFrameRateLabel(frameRate);
        }

        int frameRateIndex = Array.IndexOf(FrameRateOptions, frameRate);
        if (frameRateIndex < 0)
        {
            frameRateIndex = 0;
        }

        SetButtonInteractable(frameRatePrevButton, frameRateIndex > 0);
        SetButtonInteractable(frameRateNextButton, frameRateIndex < FrameRateOptions.Length - 1);

        string[] qualityNames = QualitySettings.names;
        int qualityIndex = GameSettings.GetQualityIndex();
        if (qualityValueText != null)
        {
            qualityValueText.text =
                qualityNames != null && qualityNames.Length > 0
                    ? qualityNames[qualityIndex].ToUpperInvariant()
                    : "N/A";
        }

        bool hasQualityOptions = qualityNames != null && qualityNames.Length > 0;
        SetButtonInteractable(qualityPrevButton, hasQualityOptions && qualityIndex > 0);
        SetButtonInteractable(
            qualityNextButton,
            hasQualityOptions && qualityIndex < qualityNames.Length - 1);

        bool fullscreen = GameSettings.GetFullscreen();
        if (fullscreenValueText != null)
        {
            fullscreenValueText.text = GameSettings.FormatFullscreenLabel(fullscreen);
        }

        SetButtonInteractable(fullscreenOffButton, fullscreen);
        SetButtonInteractable(fullscreenOnButton, !fullscreen);
    }

    private static void UpdateVolumeLabel(TextMeshProUGUI label, float value)
    {
        if (label == null)
        {
            return;
        }

        label.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    private static void SetButtonInteractable(Button button, bool interactable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = interactable;
    }
}
