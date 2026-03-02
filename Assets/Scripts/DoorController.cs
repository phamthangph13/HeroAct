using UnityEngine;

/// <summary>
/// Gắn vào GameObject cửa (Doors1h).
/// Kiểm tra khoảng cách player mỗi frame — mở/đóng animation.
/// Animation KHÔNG loop (đã tắt m_LoopTime).
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Cài đặt")]
    public float openDistance = 1.5f;
    public string playerTag = "Player";

    [Header("Tên Animation States")]
    public string openStateName = "doors1_opening";
    public string closeStateName = "doors1_closing";

    private Animator animator;
    private Transform player;
    private bool isOpen = false;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        // Bắt đầu ở trạng thái đóng — play closing ở frame cuối
        if (animator != null)
        {
            animator.Play(closeStateName, 0, 1f);
        }
    }

    private void Update()
    {
        if (player == null || animator == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= openDistance && !isOpen)
        {
            isOpen = true;
            animator.Play(openStateName, 0, 0f);
        }
        else if (distance > openDistance && isOpen)
        {
            isOpen = false;
            animator.Play(closeStateName, 0, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}
