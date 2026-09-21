using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Configuración del objeto")]
    [SerializeField] private string itemId = "item_1";
    [SerializeField] private string itemName = "Objeto";
    [SerializeField] private int requiredAmount = 1;
    [SerializeField] private Sprite sprite;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private string pickupMessage = "Objeto recogido";
    [SerializeField] private bool canBeCollected = true;

    [Header("Referencias")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private ScreenMessageController messageController;
    [SerializeField] private Collider2D pickupCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public int RequiredAmount => requiredAmount;

    private void Awake()
    {
        if (objectiveManager == null)
        {
            objectiveManager = FindAnyObjectByType<ObjectiveManager>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (pickupCollider == null)
        {
            pickupCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected)
        {
            return;
        }

        if (EarthquakeManager.Instance != null && !EarthquakeManager.Instance.IsEarthquakeActive)
        {
            return;
        }

        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (!canBeCollected)
        {
            return;
        }

        if (EarthquakeManager.Instance != null && !EarthquakeManager.Instance.IsEarthquakeActive)
        {
            return;
        }

        canBeCollected = false;

        if (objectiveManager != null)
        {
            objectiveManager.RegisterCollectedItem(itemId, itemName, pickupMessage, requiredAmount);
        }

        if (pickupSound != null && messageController != null)
        {
            AudioSource audioSource = messageController.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        Collider2D collider = pickupCollider != null ? pickupCollider : GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }
}
