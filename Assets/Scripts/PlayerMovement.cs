using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Variables de control
    private Vector2 movement;
    private bool isDefeated;
    private bool isFrozen;
    private float lastFacingDirection = 1f;
    private float freezeTimer;

    public void FreezeMovement(float duration = 0f)
    {
        isFrozen = true;
        movement = Vector2.zero;
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }

        if (duration > 0f)
        {
            freezeTimer = duration;
        }
    }

    public void UnfreezeMovement()
    {
        isFrozen = false;
        freezeTimer = 0f;
    }

    public void SetDefeatedPose()
    {
        isDefeated = true;
        isFrozen = true;
        movement = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
        }

        bool facingLeft = spriteRenderer != null && spriteRenderer.flipX;
        float rotationZ = facingLeft ? -90f : 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void Awake()
    {
        // Obtener componentes automáticos desde el GameObject
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
            {
                isFrozen = false;
            }
        }

        if (isDefeated || isFrozen)
        {
            movement = Vector2.zero;
            HandleVisuals();
            return;
        }

        if (ScreenMessageController.IsMessageFreezeActive)
        {
            movement = Vector2.zero;
            HandleVisuals();
            return;
        }

        // 1. Capturar entradas con el nuevo Input System
        movement = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x += 1f;
        }

        // Normalizar velocidad diagonal
        movement = movement.normalized;

        // 2. Gestionar la animación y el giro del Sprite
        HandleVisuals();
    }

    private void FixedUpdate()
    {
        if (isFrozen || ScreenMessageController.IsMessageFreezeActive)
        {
            return;
        }

        // 3. Aplicar física de movimiento
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleVisuals()
    {
        // Evaluar si se está moviendo en cualquier dirección (x o y)
        bool isMoving = movement.magnitude > 0.01f;

        // Actualizar parámetro en el Animator (si existe)
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }

        // Voltear Sprite según la dirección horizontal (X)
        if (spriteRenderer != null)
        {
            if (movement.x < 0)
            {
                spriteRenderer.flipX = true;
                lastFacingDirection = -1f;
            }
            else if (movement.x > 0)
            {
                spriteRenderer.flipX = false;
                lastFacingDirection = 1f;
            }
        }
    }
}