using UnityEngine;

/// <summary>
/// Plays looping background music and keeps it alive across scene loads.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private AudioSource audioSource;
    private static BackgroundMusic instance;

    private void Reset()
    {
        EnsureAudioSourceConfigured();
        ApplySerializedValues();
    }

    private void OnValidate()
    {
        EnsureAudioSourceConfigured();
        ApplySerializedValues();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSourceConfigured();
        volume = GameSettings.GetMusicVolume(volume);
        ApplySerializedValues();

        if (musicClip != null)
        {
            audioSource.Play();
            Debug.Log($"BackgroundMusic: Playing '{musicClip.name}' in loop.");
        }
        else
        {
            Debug.LogWarning("BackgroundMusic: Missing music clip.");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        ApplySerializedValues();
    }

    public void TogglePause()
    {
        if (audioSource == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    private void EnsureAudioSourceConfigured()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void ApplySerializedValues()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }
}
