using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a Canvas to auto-configure it for mobile responsiveness.
/// Sets CanvasScaler to "Scale With Screen Size" with reference resolution 1080x1920.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class MobileUISetup : MonoBehaviour
{
    [Header("Reference Resolution")]
    public Vector2 referenceResolution = new Vector2(1080f, 1920f);

    [Header("Match Mode")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f; // 0.5 = balanced scaling

    private void Awake()
    {
        ConfigureCanvas();
    }

    private void ConfigureCanvas()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        Debug.Log($"MobileUISetup: Canvas configured — Reference: {referenceResolution}, Match: {matchWidthOrHeight}");
    }
}
