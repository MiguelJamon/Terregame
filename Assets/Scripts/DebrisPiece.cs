using System.Collections;
using UnityEngine;

public class DebrisPiece : MonoBehaviour
{
    [Header("Configuración del escombro")]
    [SerializeField] private float fallSpeed = 1.8f;
    [SerializeField] private float destructionHeight = -8f;
    [SerializeField] private float lifeTime = 8f;
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageCooldown = 0.35f;

    private Rigidbody2D rb;
    private Collider2D debrisCollider;
    private DebrisSpawner ownerSpawner;
    private float nextDamageTime;

    public int DamageAmount => damageAmount;

    public void Initialize(DebrisSpawner spawner, float speed, int damage, float lifetime, float cooldown)
    {
        ownerSpawner = spawner;
        fallSpeed = speed;
        damageAmount = damage;
        lifeTime = lifetime;
        damageCooldown = cooldown;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        debrisCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.simulated = true;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }

    private void Start()
    {
        StartCoroutine(DespawnRoutine());
    }

    private void Update()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        if (transform.position.y <= destructionHeight)
        {
            DestroyDebris();
        }
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        DestroyDebris();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplyDamage(collision.collider);
    }

    private void TryApplyDamage(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null && !other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + damageCooldown;
        playerHealth = playerHealth != null ? playerHealth : other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            DestroyDebris();
        }
    }

    private void DestroyDebris()
    {
        if (ownerSpawner != null)
        {
            ownerSpawner.NotifyDebrisDestroyed(this);
        }

        Destroy(gameObject);
    }
}
