using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool isValidDuringEarthquake = true;
    [SerializeField] private bool showProtectionMessage = true;
    [SerializeField] private string protectionMessage = "Estás protegido";
    [SerializeField] private bool allowExitMessage = false;
    [SerializeField] private bool isActive = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer safeZoneRenderer;
    [SerializeField] private Color safeZoneColor = new Color(0.3f, 1f, 0.45f, 0.85f);

    [Header("Referencias")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private ScreenMessageController messageController;
    [SerializeField] private Collider2D safeZoneCollider;
    [SerializeField] private float blockedMessageCooldown = 0.4f;

    private bool playerInside;
    private float lastBlockedMessageTime = float.NegativeInfinity;

    public bool IsPlayerInside => playerInside;
    public bool IsValidDuringEarthquake => isValidDuringEarthquake;

    private void Awake()
    {
        if (safeZoneCollider == null)
        {
            safeZoneCollider = GetComponent<Collider2D>();
        }

        if (safeZoneRenderer == null)
        {
            safeZoneRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (safeZoneRenderer != null)
        {
            safeZoneRenderer.color = safeZoneColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || other == null)
        {
            return;
        }

        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            playerInside = true;

            if (objectiveManager != null && objectiveManager.WaitingForSafeZone)
            {
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.FreezeMovement(0.4f);
                }

                objectiveManager.CompleteObjectiveFromSafeZone();
                return;
            }

            if (showProtectionMessage && isValidDuringEarthquake && messageController != null)
            {
                float timeSinceLastBlocked = Time.unscaledTime - lastBlockedMessageTime;
                if (timeSinceLastBlocked >= blockedMessageCooldown)
                {
                    lastBlockedMessageTime = Time.unscaledTime;

                    PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.FreezeMovement(blockedMessageCooldown);
                    }

                    messageController.ShowMessage(new ScreenMessageController.MessageSettings
                    {
                        text = "Faltan objetos.",
                        displayDuration = 0.8f,
                        fontSize = 28f,
                        textColor = Color.white,
                        backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                        anchoredPosition = new Vector2(0f, -170f),
                        transparency = 1f,
                        fadeInDuration = 0f,
                        fadeOutDuration = 0f,
                        freezeGameplay = false
                    });
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            playerInside = false;

            if (allowExitMessage && messageController != null)
            {
                messageController.ShowMessage(new ScreenMessageController.MessageSettings
                {
                    text = "Saliste de la zona segura",
                    displayDuration = 1.2f,
                    fontSize = 24f,
                    textColor = Color.white,
                    backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                    anchoredPosition = new Vector2(0f, -170f)
                });
            }
        }
    }

    private void EvaluateZone()
    {
        if (!isValidDuringEarthquake || objectiveManager == null || objectiveManager.WaitingForSafeZone)
        {
            return;
        }

        if (showProtectionMessage && messageController != null)
        {
            messageController.ShowMessage(new ScreenMessageController.MessageSettings
            {
                text = protectionMessage,
                displayDuration = 1.2f,
                fontSize = 26f,
                textColor = Color.green,
                backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                anchoredPosition = new Vector2(0f, -170f),
                transparency = 1f,
                fadeInDuration = 0f,
                fadeOutDuration = 0f,
                freezeGameplay = false
            });
        }
    }
}
