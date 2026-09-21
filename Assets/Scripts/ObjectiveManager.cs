using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjectiveDefinition
    {
        public string itemId = "item_1";
        public string itemName = "Objeto";
        public int requiredAmount = 1;
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup objectiveCanvasGroup;
    [SerializeField] private ScreenMessageController screenMessageController;

    [Header("Objetivos")]
    [SerializeField] private string objectiveStartMessage = "Recolecta los objetos necesarios y llega a un lugar seguro.";
    [SerializeField] private string allItemsCollectedMessage = "Objetivos completados. Ve a la zona segura.";
    [SerializeField] private string safeZoneReachedMessage = "¡Has llegado a la zona segura!";
    [SerializeField] private string currentObjective = "Recolecta los objetos necesarios y llega a un lugar seguro.";
    [SerializeField] private List<ObjectiveDefinition> requiredItems = new List<ObjectiveDefinition>();

    private readonly Dictionary<string, int> collectedCounts = new Dictionary<string, int>();
    private bool objectiveCompleted;
    private bool waitingForSafeZone;
    private Coroutine statusClearRoutine;

    public bool ObjectiveCompleted => objectiveCompleted;
    public bool WaitingForSafeZone => waitingForSafeZone;

    private void Start()
    {
        if (objectiveCanvasGroup == null)
        {
            objectiveCanvasGroup = GetComponent<CanvasGroup>();
        }

        ConfigureFixedObjectiveLayout();
        SetObjectiveUIVisible(false);
        UpdateObjectiveUI();
    }

    private void ConfigureFixedObjectiveLayout()
    {
        if (objectiveText != null)
        {
            objectiveText.enableAutoSizing = false;
            objectiveText.textWrappingMode = TextWrappingModes.Normal;
            objectiveText.overflowMode = TextOverflowModes.Overflow;
            objectiveText.alignment = TextAlignmentOptions.Center;
        }

        if (progressText != null)
        {
            progressText.enableAutoSizing = false;
            progressText.textWrappingMode = TextWrappingModes.Normal;
            progressText.overflowMode = TextOverflowModes.Overflow;
            progressText.alignment = TextAlignmentOptions.Left;
        }

        if (statusText != null)
        {
            statusText.enableAutoSizing = false;
            statusText.textWrappingMode = TextWrappingModes.Normal;
            statusText.overflowMode = TextOverflowModes.Overflow;
            statusText.alignment = TextAlignmentOptions.Center;
        }
    }

    public void SetCurrentObjective(string objective)
    {
        currentObjective = objective;
        UpdateObjectiveUI();
    }

    public string ObjectiveStartMessage => objectiveStartMessage;
    public string AllItemsCollectedMessage => allItemsCollectedMessage;
    public string SafeZoneReachedMessage => safeZoneReachedMessage;

    public void SetObjectiveUIVisible(bool visible)
    {
        var texts = new[] { objectiveText, progressText, statusText };

        foreach (var text in texts)
        {
            if (text != null)
            {
                text.gameObject.SetActive(visible);
            }
        }

        if (objectiveCanvasGroup != null)
        {
            objectiveCanvasGroup.alpha = visible ? 1f : 0f;
            objectiveCanvasGroup.interactable = visible;
            objectiveCanvasGroup.blocksRaycasts = visible;
        }
    }

    public void SetRequiredItems(List<ObjectiveDefinition> items)
    {
        requiredItems = items ?? new List<ObjectiveDefinition>();
        collectedCounts.Clear();
        objectiveCompleted = false;
        waitingForSafeZone = false;
        UpdateObjectiveUI();
    }

    public void CompleteObjectiveFromSafeZone()
    {
        if (objectiveCompleted || !waitingForSafeZone)
        {
            return;
        }

        objectiveCompleted = true;
        waitingForSafeZone = false;

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        SetCurrentObjective(safeZoneReachedMessage);
        SetObjectiveUIVisible(false);

        ScreenMessageController.MessageSettings completionMessage = null;

        if (screenMessageController != null)
        {
            completionMessage = new ScreenMessageController.MessageSettings
            {
                text = string.IsNullOrWhiteSpace(screenMessageController.victoryMessageText) ? "Objetivos completados" : screenMessageController.victoryMessageText,
                displayDuration = screenMessageController.victoryDisplayDuration,
                fontSize = screenMessageController.victoryFontSize,
                textColor = Color.green,
                backgroundColor = new Color(0f, 0f, 0f, 0.55f),
                anchoredPosition = new Vector2(0f, 0f),
                transparency = 1f,
                fadeInDuration = 0f,
                fadeOutDuration = 0f,
                freezeGameplay = false
            };
            screenMessageController.ShowVictoryMessage(completionMessage.text, completionMessage.displayDuration);
        }

        if (EarthquakeManager.Instance != null)
        {
            float delay = completionMessage != null ? completionMessage.displayDuration + 0.65f : 1.95f;
            EarthquakeManager.Instance.BeginObjectiveVictoryCleanup(delay);
        }
    }

    public void RegisterCollectedItem(string itemId, string itemName, string pickupMessage, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return;
        }

        if (!collectedCounts.ContainsKey(itemId))
        {
            collectedCounts[itemId] = 0;
        }

        collectedCounts[itemId] += amount;

        bool objectiveFinishedNow = IsObjectiveComplete();

        if (!objectiveFinishedNow && screenMessageController != null && ShouldShowPickupMessage(pickupMessage))
        {
            ScreenMessageController.MessageSettings message = new ScreenMessageController.MessageSettings
            {
                text = pickupMessage,
                displayDuration = 1f,
                fontSize = 26f,
                textColor = Color.white,
                backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                anchoredPosition = new Vector2(0f, -180f),
                transparency = 1f,
                fadeInDuration = 0.1f,
                fadeOutDuration = 0.25f,
                freezeGameplay = false
            };
            screenMessageController.ShowMessage(message);
        }

        if (objectiveFinishedNow)
        {
            waitingForSafeZone = true;

            if (statusText != null)
            {
                statusText.text = string.Empty;
            }

            SetCurrentObjective(allItemsCollectedMessage);
            UpdateObjectiveUI();
            return;
        }

        ShowStatusText(string.IsNullOrEmpty(itemName) ? "Objetivo en progreso" : itemName + " recogido", 1f);
        UpdateObjectiveUI();
    }

    private bool ShouldShowPickupMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return !string.Equals(message.Trim(), "Objeto recogido", System.StringComparison.OrdinalIgnoreCase);
    }

    private void ShowStatusText(string text, float duration)
    {
        if (statusText == null)
        {
            return;
        }

        if (statusClearRoutine != null)
        {
            StopCoroutine(statusClearRoutine);
        }

        statusText.text = text;
        statusClearRoutine = StartCoroutine(ClearStatusTextAfterDelay(duration));
    }

    private System.Collections.IEnumerator ClearStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (statusText != null)
        {
            statusText.text = string.Empty;
        }
        statusClearRoutine = null;
    }

    public bool IsObjectiveComplete()
    {
        foreach (var item in requiredItems)
        {
            int collected = GetCollectedAmount(item.itemId);
            if (collected < item.requiredAmount)
            {
                return false;
            }
        }

        return requiredItems.Count > 0;
    }

    public int GetCollectedAmount(string itemId)
    {
        if (collectedCounts.TryGetValue(itemId, out var count))
        {
            return count;
        }

        return 0;
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = currentObjective;
        }

        if (progressText != null)
        {
            progressText.text = BuildProgressText();
        }
    }

    private string BuildProgressText()
    {
        if (requiredItems.Count == 0)
        {
            return "Sin objetos requeridos";
        }

        string text = "";
        for (int i = 0; i < requiredItems.Count; i++)
        {
            var item = requiredItems[i];
            int collected = GetCollectedAmount(item.itemId);
            text += "- " + item.itemName + ": " + collected + "/" + item.requiredAmount;

            if (i < requiredItems.Count - 1)
            {
                text += "\n";
            }
        }

        return text;
    }
}
