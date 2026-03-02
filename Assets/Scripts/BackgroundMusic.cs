using UnityEngine;

/// <summary>
/// Phát nhạc nền loop liên tục. Tồn tại xuyên suốt các scene (DontDestroyOnLoad).
/// Kéo file music vào field "Music Clip" trong Inspector.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip; // Kéo file music.mp3 vào đây

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private AudioSource audioSource;

    // Singleton — chỉ tồn tại 1 instance duy nhất
    private static BackgroundMusic instance;

    private void Awake()
    {
        // Nếu đã có instance khác → hủy cái mới
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Không bị hủy khi chuyển scene

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true;        // Loop liên tục
        audioSource.playOnAwake = false;

        // Phát nhạc
        if (musicClip != null)
        {
            audioSource.Play();
            Debug.Log($"BackgroundMusic: Đang phát '{musicClip.name}' (loop)");
        }
        else
        {
            Debug.LogWarning("BackgroundMusic: Chưa gán Music Clip!");
        }
    }

    /// <summary>
    /// Thay đổi âm lượng nhạc nền (gọi từ Settings)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Tạm dừng / tiếp tục nhạc nền
    /// </summary>
    public void TogglePause()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.UnPause();
    }
}
