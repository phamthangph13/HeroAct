using UnityEngine;

/// <summary>
/// Hiệu ứng text damage bay lên rồi mất.
/// Tự gắn vào popup object.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 1f;       // Tốc độ bay lên
    public float fadeSpeed = 2f;        // Tốc độ mờ dần
    public float lifetime = 0.8f;       // Thời gian tồn tại

    private float timer;
    private TextMesh textMesh;

    private void Start()
    {
        textMesh = GetComponent<TextMesh>();
        timer = lifetime;
    }

    private void Update()
    {
        // Bay lên
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Mờ dần
        timer -= Time.deltaTime;
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = Mathf.Clamp01(timer / lifetime);
            textMesh.color = c;
        }

        // Hết thời gian → xóa
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
