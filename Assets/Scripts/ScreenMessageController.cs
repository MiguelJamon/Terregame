using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScreenMessageController : MonoBehaviour
{
    [System.Serializable]
    public class MessageSettings
    {
        public string text = "Comenzó a temblar";
        public float displayDuration = 2f;
        public float fontSize = 42f;
        public TMP_FontAsset font;
        public Color textColor = Color.white;
        public Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
        public Vector2 anchoredPosition = new Vector2(0f, 120f);
        public float transparency = 1f;
        public float fadeInDuration = 0.2f;
        public float fadeOutDuration = 0.2f;
        public bool freezeGameplay = false;
        public AudioClip sound;
        public AnimationCurve appearanceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve disappearanceCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    [Header("Referencias")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public CanvasGroup messageCanvasGroup;
    public RectTransform messageRoot;
    public AudioSource audioSource;
    [Header("Posicionamiento manual")]
    public bool useManualCanvasPlacement = true;
    public bool allowCodeDrivenPosition = false;
    public Vector2 manualMessageOffset = Vector2.zero;

    [Header("Mensaje de victoria")]
    public bool useVictoryCustomPosition = true;
    public Vector2 victoryMessagePosition = new Vector2(0f, 0f);
    public float victoryFontSize = 72f;
    public string victoryMessageText = "Objetivos completados";
    public float victoryDisplayDuration = 1.8f;

    [Header("Mensaje de derrota")]
    public string defeatMessageText = "No cumpliste la misión. El terremoto te venció.";
    public float defeatMessageFontSize = 34f;
    public Vector2 defeatMessagePosition = Vector2.zero;
    public float defeatTextWidth = 900f;
    public float defeatTextHeight = 180f;
    public Color defeatTextColor = Color.white;
    public Color defeatBackgroundColor = new Color(1f, 0.18f, 0.18f, 1f);
    public bool defeatUsesFullScreenRed = true;
    [Range(0f, 1f)] public float defeatBackgroundAlpha = 0.25f;
    public bool defeatCenterText = true;

    public bool UseManualCanvasPlacement
    {
        get => useManualCanvasPlacement;
        set => useManualCanvasPlacement = value;
    }

    public Vector2 ManualMessageOffset
    {
        get => manualMessageOffset;
        set => manualMessageOffset = value;
    }

    public bool UseVictoryCustomPosition
    {
        get => useVictoryCustomPosition;
        set => useVictoryCustomPosition = value;
    }

    public Vector2 VictoryMessagePosition
    {
        get => victoryMessagePosition;
        set => victoryMessagePosition = value;
    }

    public float VictoryFontSize
    {
        get => victoryFontSize;
        set => victoryFontSize = value;
    }

    public string DefeatMessageText
    {
        get => defeatMessageText;
        set => defeatMessageText = value;
    }

    public float DefeatMessageFontSize
    {
        get => defeatMessageFontSize;
        set => defeatMessageFontSize = value;
    }

    public Vector2 DefeatMessagePosition
    {
        get => defeatMessagePosition;
        set => defeatMessagePosition = value;
    }

    public Color DefeatTextColor
    {
        get => defeatTextColor;
        set => defeatTextColor = value;
    }

    public Color DefeatBackgroundColor
    {
        get => defeatBackgroundColor;
        set => defeatBackgroundColor = value;
    }

    public float DefeatTextWidth
    {
        get => defeatTextWidth;
        set => defeatTextWidth = value;
    }

    public float DefeatTextHeight
    {
        get => defeatTextHeight;
        set => defeatTextHeight = value;
    }

    public bool DefeatUsesFullScreenRed
    {
        get => defeatUsesFullScreenRed;
        set => defeatUsesFullScreenRed = value;
    }

    public float DefeatBackgroundAlpha
    {
        get => defeatBackgroundAlpha;
        set => defeatBackgroundAlpha = value;
    }

    public bool DefeatCenterText
    {
        get => defeatCenterText;
        set => defeatCenterText = value;
    }

    [Header("Configuración predeterminada")]
    [SerializeField] private MessageSettings defaultSettings = new MessageSettings();
    [SerializeField] private float typingCharacterDelay = 0.02f;

    private Coroutine currentRoutine;
    private string currentFullText;
    private bool skipTyping;
    private static bool isMessageFreezeActive;
    private Vector2 defaultMessageTextSize;
    private Vector2 defaultBackgroundSize;
    private float defaultMessageFontSize;

    public event Action MessageFinished;

    public static bool IsMessageFreezeActive => isMessageFreezeActive;

    private void Awake()
    {
        if (messageText == null)
        {
            messageText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponentInChildren<Image>();
        }

        if (messageCanvasGroup == null)
        {
            messageCanvasGroup = GetComponent<CanvasGroup>();
            if (messageCanvasGroup == null && messageRoot != null)
            {
                messageCanvasGroup = messageRoot.GetComponent<CanvasGroup>();
            }
        }

        if (messageRoot == null)
        {
            if (messageText != null)
            {
                messageRoot = messageText.rectTransform;
            }
            else if (backgroundImage != null)
            {
                messageRoot = backgroundImage.rectTransform;
            }
        }

        if (messageText != null && messageText.rectTransform != null)
        {
            defaultMessageTextSize = messageText.rectTransform.sizeDelta;
            defaultMessageFontSize = messageText.fontSize;
        }
        else
        {
            defaultMessageTextSize = new Vector2(600f, 120f);
            defaultMessageFontSize = 42f;
        }

        if (backgroundImage != null && backgroundImage.rectTransform != null)
        {
            defaultBackgroundSize = backgroundImage.rectTransform.sizeDelta;
        }
        else
        {
            defaultBackgroundSize = new Vector2(760f, 180f);
        }

        ConfigureFixedTextLayout();
        ApplySettings(defaultSettings, true);
        HideMessage();
    }

    private void ConfigureFixedTextLayout()
    {
        if (messageText != null)
        {
            messageText.enableAutoSizing = false;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.overflowMode = TextOverflowModes.Overflow;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.raycastTarget = false;
        }
    }

    private void ApplyTextWrapAndFit(TextMeshProUGUI tmp, float maxWidth, float maxHeight)
    {
        if (tmp == null)
        {
            return;
        }

        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;

        float currentFontSize = tmp.fontSize;
        const float minFontSize = 18f;

        for (int i = 0; i < 12; i++)
        {
            tmp.ForceMeshUpdate();

            float textWidth = tmp.preferredWidth;
            float textHeight = tmp.preferredHeight;

            if (textWidth <= maxWidth && textHeight <= maxHeight)
            {
                return;
            }

            if (currentFontSize <= minFontSize)
            {
                break;
            }

            currentFontSize = Mathf.Max(minFontSize, currentFontSize - 2f);
            tmp.fontSize = currentFontSize;
        }
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(currentFullText) && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            skipTyping = true;
        }
    }

    public void ShowMessage(MessageSettings settings)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (settings == null)
        {
            settings = defaultSettings;
        }

        if (isMessageFreezeActive && !settings.freezeGameplay)
        {
            return;
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        ResetMessageVisualState();

        isMessageFreezeActive = settings.freezeGameplay;
        if (settings.freezeGameplay)
        {
            Time.timeScale = 0f;
        }

        currentFullText = settings.text;
        skipTyping = false;

        ApplySettings(settings, false);

        currentRoutine = StartCoroutine(ShowMessageRoutine(settings));
    }

    public void ShowMessage(string text, float displayDuration = 2f)
    {
        MessageSettings settings = new MessageSettings
        {
            text = text,
            displayDuration = displayDuration,
            fontSize = defaultSettings.fontSize,
            font = defaultSettings.font,
            textColor = defaultSettings.textColor,
            backgroundColor = defaultSettings.backgroundColor,
            anchoredPosition = defaultSettings.anchoredPosition,
            transparency = defaultSettings.transparency,
            fadeInDuration = defaultSettings.fadeInDuration,
            fadeOutDuration = defaultSettings.fadeOutDuration,
            sound = defaultSettings.sound
        };

        ShowMessage(settings);
    }

    public void ShowVictoryMessage(string text, float displayDuration = 1.2f)
    {
        var previousUseManual = useManualCanvasPlacement;
        var previousOffset = manualMessageOffset;

        if (useVictoryCustomPosition)
        {
            useManualCanvasPlacement = true;
            manualMessageOffset = victoryMessagePosition;
        }

        MessageSettings settings = new MessageSettings
        {
            text = text,
            displayDuration = displayDuration,
            fontSize = victoryFontSize,
            font = defaultSettings.font,
            textColor = Color.green,
            backgroundColor = new Color(0f, 0f, 0f, 0.55f),
            anchoredPosition = victoryMessagePosition,
            transparency = 1f,
            fadeInDuration = 0f,
            fadeOutDuration = 0f,
            freezeGameplay = false
        };

        ShowMessage(settings);

        useManualCanvasPlacement = previousUseManual;
        manualMessageOffset = previousOffset;
    }

    private void RestoreNormalPanelLayout()
    {
        if (backgroundImage == null || backgroundImage.rectTransform == null)
        {
            return;
        }

        backgroundImage.sprite = null;
        backgroundImage.raycastTarget = false;
        backgroundImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        backgroundImage.rectTransform.offsetMin = Vector2.zero;
        backgroundImage.rectTransform.offsetMax = Vector2.zero;
        backgroundImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ApplySettings(MessageSettings settings, bool hideImmediately)
    {
        if (messageText != null)
        {
            messageText.text = settings.text;
            messageText.font = settings.font != null ? settings.font : messageText.font;
            messageText.fontSize = settings.fontSize;
            messageText.enableAutoSizing = false;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.overflowMode = TextOverflowModes.Overflow;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = new Color(settings.textColor.r, settings.textColor.g, settings.textColor.b, settings.transparency);

            if (messageText.rectTransform != null)
            {
                float targetWidth = Mathf.Clamp(settings.fontSize * 12f, 420f, 760f);
                float targetHeight = Mathf.Clamp(settings.fontSize * 2.8f, 110f, 220f);
                messageText.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
                ApplyTextWrapAndFit(messageText, targetWidth, targetHeight);
            }
        }

        if (backgroundImage != null)
        {
            RestoreNormalPanelLayout();
            backgroundImage.color = new Color(
                settings.backgroundColor.r,
                settings.backgroundColor.g,
                settings.backgroundColor.b,
                Mathf.Clamp01(settings.transparency * settings.backgroundColor.a));

            if (backgroundImage.rectTransform != null)
            {
                float width = messageText != null ? Mathf.Clamp(messageText.rectTransform.rect.width + 80f, 520f, 900f) : 600f;
                float height = messageText != null ? Mathf.Clamp(messageText.rectTransform.rect.height + 50f, 130f, 260f) : 180f;
                backgroundImage.rectTransform.sizeDelta = new Vector2(width, height);
            }
        }

        if (messageRoot != null && allowCodeDrivenPosition)
        {
            if (useManualCanvasPlacement)
            {
                messageRoot.anchoredPosition = manualMessageOffset;
            }
            else
            {
                messageRoot.anchoredPosition = settings.anchoredPosition;
            }
        }

        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = hideImmediately ? 0f : settings.transparency;
        }
    }

    private void ResetMessageVisualState()
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.fontSize = defaultMessageFontSize;
            if (messageText.rectTransform != null)
            {
                messageText.rectTransform.sizeDelta = defaultMessageTextSize;
            }
        }

        if (backgroundImage != null)
        {
            RestoreNormalPanelLayout();
            backgroundImage.enabled = false;
            backgroundImage.gameObject.SetActive(false);
            if (backgroundImage.rectTransform != null)
            {
                backgroundImage.rectTransform.sizeDelta = defaultBackgroundSize;
            }
        }

        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 0f;
            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator ShowMessageRoutine(MessageSettings settings)
    {
        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 0f;
            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }

        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
        }

        if (messageCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < settings.fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(settings.fadeInDuration, 0.01f));
                float curveValue = settings.appearanceCurve.Evaluate(t);
                messageCanvasGroup.alpha = Mathf.Lerp(0f, settings.transparency, curveValue);
                yield return null;
            }
            messageCanvasGroup.alpha = settings.transparency;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
            for (int i = 0; i <= currentFullText.Length; i++)
            {
                if (skipTyping)
                {
                    messageText.text = currentFullText;
                    break;
                }

                messageText.text = currentFullText.Substring(0, i);
                yield return new WaitForSecondsRealtime(0.04f);
            }
        }

        if (audioSource != null && settings.sound != null)
        {
            audioSource.PlayOneShot(settings.sound);
        }

        yield return new WaitForSecondsRealtime(settings.displayDuration);

        if (messageCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < settings.fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(settings.fadeOutDuration, 0.01f));
                float curveValue = settings.disappearanceCurve.Evaluate(t);
                messageCanvasGroup.alpha = Mathf.Lerp(settings.transparency, 0f, curveValue);
                yield return null;
            }

            messageCanvasGroup.alpha = 0f;
            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }

        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        currentRoutine = null;
        isMessageFreezeActive = false;
        if (settings.freezeGameplay && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        MessageFinished?.Invoke();
    }

    public void ShowFailureScreen(string text = null)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        string finalText = string.IsNullOrWhiteSpace(text) ? defeatMessageText : text;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = finalText;
            messageText.fontSize = defeatMessageFontSize;
            messageText.enableAutoSizing = false;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.overflowMode = TextOverflowModes.Overflow;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = defeatTextColor;
            messageText.raycastTarget = false;
            if (messageText.rectTransform != null)
            {
                if (defeatCenterText)
                {
                    messageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    messageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    messageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                }

                messageText.rectTransform.sizeDelta = new Vector2(defeatTextWidth, defeatTextHeight);
                messageText.rectTransform.anchoredPosition = defeatMessagePosition;
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(defeatUsesFullScreenRed || defeatBackgroundAlpha > 0.01f);
            backgroundImage.enabled = defeatUsesFullScreenRed || defeatBackgroundAlpha > 0.01f;
            backgroundImage.raycastTarget = false;
            float redAlpha = Mathf.Clamp01(defeatBackgroundAlpha);
            backgroundImage.color = new Color(
                defeatBackgroundColor.r,
                defeatBackgroundColor.g,
                defeatBackgroundColor.b,
                redAlpha);

            if (defeatUsesFullScreenRed)
            {
                if (backgroundImage.sprite == null)
                {
                    Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    backgroundImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                }

                if (backgroundImage.rectTransform != null)
                {
                    backgroundImage.rectTransform.anchorMin = Vector2.zero;
                    backgroundImage.rectTransform.anchorMax = Vector2.one;
                    backgroundImage.rectTransform.offsetMin = Vector2.zero;
                    backgroundImage.rectTransform.offsetMax = Vector2.zero;
                    backgroundImage.rectTransform.sizeDelta = Vector2.zero;
                }
            }
            else if (backgroundImage.rectTransform != null)
            {
                backgroundImage.rectTransform.sizeDelta = new Vector2(defeatTextWidth + 60f, defeatTextHeight + 40f);
                backgroundImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                backgroundImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                backgroundImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                backgroundImage.rectTransform.anchoredPosition = defeatMessagePosition;
            }
        }

        if (messageRoot != null)
        {
            messageRoot.anchorMin = Vector2.zero;
            messageRoot.anchorMax = Vector2.one;
            messageRoot.offsetMin = Vector2.zero;
            messageRoot.offsetMax = Vector2.zero;
        }

        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 1f;
            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }

        currentFullText = string.Empty;
        skipTyping = true;
        isMessageFreezeActive = true;
    }

    public void HideMessage()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        ResetMessageVisualState();
        currentFullText = string.Empty;
        skipTyping = true;

        isMessageFreezeActive = false;
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
}
