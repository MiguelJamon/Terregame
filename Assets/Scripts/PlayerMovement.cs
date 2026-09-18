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

    private void Awake()
    {
        // Obtener componentes automáticos desde el GameObject
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // BLOQUEO: Si hay un diálogo activo, frenamos al personaje y cancelamos las animaciones
        if (UITextManager.Instance != null && UITextManager.Instance.EstaEnInteraccion)
        {
            movement = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }

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
        // BLOQUEO: Si hay un diálogo activo, detenemos el cuerpo físico
        if (UITextManager.Instance != null && UITextManager.Instance.EstaEnInteraccion)
        {
            rb.linearVelocity = Vector2.zero;
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
                spriteRenderer.flipX = true; // Mira a la izquierda
            }
            else if (movement.x > 0)
            {
                spriteRenderer.flipX = false; // Mira a la derecha
            }
        }
    }
}