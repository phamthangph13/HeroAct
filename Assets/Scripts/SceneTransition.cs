using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn vào cửa/khu vực chuyển scene.
/// Khi player bước vào vùng trigger → load scene mới.
/// Cần Player có Tag = "Player".
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Scene đích")]
    public string targetSceneName; // Tên scene muốn chuyển đến (ví dụ: "World1")

    [Header("Vị trí spawn ở scene mới (tùy chọn)")]
    public string spawnPointName; // Tên spawn point ở scene mới

    [Header("Cài đặt")]
    public string playerTag = "Player";
    public float triggerRadius = 1f;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Tạo trigger zone nếu chưa có
        BoxCollider2D triggerZone = null;
        foreach (var col in GetComponents<BoxCollider2D>())
        {
            if (col.isTrigger)
            {
                triggerZone = col;
                break;
            }
        }

        if (triggerZone == null)
        {
            triggerZone = gameObject.AddComponent<BoxCollider2D>();
            triggerZone.isTrigger = true;
            triggerZone.size = new Vector2(triggerRadius, triggerRadius);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        if (other.CompareTag(playerTag))
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("SceneTransition: Chưa gán Target Scene Name!");
                return;
            }

            isTransitioning = true;

            // Lưu spawn point nếu có
            if (!string.IsNullOrEmpty(spawnPointName))
            {
                PlayerPrefs.SetString("SpawnPoint", spawnPointName);
            }

            Debug.Log($"SceneTransition: Chuyển đến scene '{targetSceneName}'");
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
