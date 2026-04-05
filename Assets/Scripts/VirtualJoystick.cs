using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform background; // Joystick background circle
    [SerializeField] private RectTransform handle;     // Joystick handle/knob

    [Header("Settings")]
    [SerializeField] private float handleRange = 1f;   // How far handle can move (1 = edge of background)
    [SerializeField] private float deadZone = 0.1f;    // Minimum input threshold

    private Vector2 inputDirection = Vector2.zero;
    private Canvas canvas;
    private Camera cam;

    /// <summary>
    /// Normalized input direction from the joystick. Use this in PlayerMovement.
    /// </summary>
    public Vector2 InputDirection => inputDirection;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = canvas.worldCamera;
        }

        // Start with handle centered
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void OnDisable()
    {
        ResetHandle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // Convert screen point to local point in background RectTransform
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, cam, out localPoint))
        {
            return;
        }

        // Normalize to -1..1 range based on background size
        Vector2 backgroundSize = background.sizeDelta;
        float radius = Mathf.Min(backgroundSize.x, backgroundSize.y) * 0.5f;
        Vector2 normalizedInput = new Vector2(
            localPoint.x / radius,
            localPoint.y / radius
        );

        // Clamp magnitude
        if (normalizedInput.magnitude > 1f)
        {
            normalizedInput.Normalize();
        }

        // Apply dead zone
        if (normalizedInput.magnitude < deadZone)
        {
            normalizedInput = Vector2.zero;
        }

        inputDirection = normalizedInput;

        // Move handle visual
        handle.anchoredPosition = normalizedInput * radius * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHandle();
    }

    private void ResetHandle()
    {
        inputDirection = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
