using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossVictoryPanelController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text subtitleText;

    private bool buttonsBound;

    private void Awake()
    {
        CacheReferences();
        BindButtons();
    }

    private void OnEnable()
    {
        CacheReferences();
        BindButtons();
    }

    public void Configure(CanvasGroup canvasGroupReference, Button continueButtonReference, Button closeButtonReference, TMP_Text subtitleReference)
    {
        canvasGroup = canvasGroupReference;
        continueButton = continueButtonReference;
        closeButton = closeButtonReference;
        subtitleText = subtitleReference;
        buttonsBound = false;
        BindButtons();
    }

    public void Show()
    {
        CacheReferences();
        BindButtons();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    private void CacheReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (continueButton == null)
        {
            Transform continueTransform = transform.Find("MainBoard/ContinueButton");
            if (continueTransform != null)
            {
                continueButton = continueTransform.GetComponent<Button>();
            }
        }

        if (closeButton == null)
        {
            Transform closeTransform = transform.Find("MainBoard/CloseButton");
            if (closeTransform != null)
            {
                closeButton = closeTransform.GetComponent<Button>();
            }
        }

        if (subtitleText == null)
        {
            Transform subtitleTransform = transform.Find("MainBoard/InnerBoard/Subtitle");
            if (subtitleTransform != null)
            {
                subtitleText = subtitleTransform.GetComponent<TMP_Text>();
            }
        }
    }

    private void BindButtons()
    {
        if (buttonsBound)
        {
            return;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Hide);
            continueButton.onClick.AddListener(Hide);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        buttonsBound = true;
    }
}
