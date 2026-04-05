using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class CameraFollow : MonoBehaviour
{
    private const float DefaultPortraitOrthographicSize = 2.8f;

    [Header("Target")]
    public Transform target; // Assign Player here

    [Header("Follow Settings")]
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Z=-10 for 2D camera

    [Header("Bounds (Optional)")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Runtime Safety")]
    [SerializeField] private bool normalizeTransformOnEnable = true;
    [SerializeField] private bool ensureMainCameraTag = true;
    [SerializeField] private bool disablePixelPerfectInPortrait = false;
    [SerializeField] private float portraitOrthographicSize = 0f;

    private Camera cachedCamera;
    private PixelPerfectCamera pixelPerfectCamera;
    private bool hasSnappedToTarget;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        PrepareCameraTransform();
        SnapToTarget();
    }

    private void OnEnable()
    {
        PrepareCameraTransform();
        SnapToTarget();
    }

    private void OnValidate()
    {
        CacheComponents();
        PrepareCameraTransform();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            PrepareCameraTransform();
        }
    }

    private void LateUpdate()
    {
        PrepareCameraTransform();

        if (target == null) return;

        Vector3 desiredPosition = GetDesiredPosition();

        if (!hasSnappedToTarget)
        {
            transform.position = desiredPosition;
            hasSnappedToTarget = true;
            return;
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.deltaTime
        );

        transform.position = ClampPosition(smoothedPosition);
    }

    private void PrepareCameraTransform()
    {
        CacheComponents();

        if (normalizeTransformOnEnable)
        {
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }

            transform.localScale = Vector3.one;
        }

        ApplyPixelPerfectCompatibility();

        if (ensureMainCameraTag && cachedCamera != null && !CompareTag("MainCamera"))
        {
            gameObject.tag = "MainCamera";
        }
    }

    private void CacheComponents()
    {
        if (cachedCamera == null)
        {
            cachedCamera = GetComponent<Camera>();
        }

        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        }
    }

    private void ApplyPixelPerfectCompatibility()
    {
        if (pixelPerfectCamera == null)
        {
            return;
        }

        bool shouldDisablePixelPerfect = ShouldDisablePixelPerfect();
        if (shouldDisablePixelPerfect)
        {
            if (pixelPerfectCamera.enabled)
            {
                pixelPerfectCamera.enabled = false;
            }

            if (cachedCamera != null)
            {
                float desiredSize = portraitOrthographicSize > 0f
                    ? portraitOrthographicSize
                    : DefaultPortraitOrthographicSize;

                if (!Mathf.Approximately(cachedCamera.orthographicSize, desiredSize))
                {
                    cachedCamera.orthographicSize = desiredSize;
                }
            }
        }
    }

    private bool ShouldDisablePixelPerfect()
    {
        if (pixelPerfectCamera == null)
        {
            return false;
        }

        bool isPortraitViewport =
            Screen.height > Screen.width ||
            (cachedCamera != null && cachedCamera.pixelHeight > cachedCamera.pixelWidth);

        bool hasLandscapeReference = pixelPerfectCamera.refResolutionX > pixelPerfectCamera.refResolutionY;
        return isPortraitViewport && (disablePixelPerfectInPortrait || hasLandscapeReference);
    }

    private void SnapToTarget()
    {
        if (target == null) return;

        transform.position = GetDesiredPosition();
        hasSnappedToTarget = true;
    }

    private Vector3 GetDesiredPosition()
    {
        return ClampPosition(target.position + offset);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        if (useBounds)
        {
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        }

        return position;
    }
}
