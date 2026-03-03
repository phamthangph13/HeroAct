using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cổng chuyển scene. Đặt Collider2D (isTrigger) tại cửa.
/// Player bước vào → chuyển sang scene khác.
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Header("Scene đích")]
    public string targetScene = "World_Zombie";

    [Header("Hiệu ứng")]
    public float delay = 0.5f;  // Delay trước khi chuyển

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log($"Chuyển sang {targetScene}!");

            // Chuyển scene sau delay
            StartCoroutine(LoadAfterDelay());
        }
    }

    private System.Collections.IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(targetScene);
    }

    // Hiện gizmo trong Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            Gizmos.DrawCube(
                transform.position + (Vector3)box.offset,
                box.size * transform.lossyScale
            );
    }
}
