using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float interactDistance = 1.2f;
    [SerializeField] private LayerMask interactableLayer;

    private Vector2 lastFacingDirection = Vector2.down;

    private void Update()
    {
        // SI ESTÁ EN INTERACCIÓN: Bloquea el movimiento y la detección de nuevas interacciones
        if (UITextManager.Instance != null && UITextManager.Instance.EstaEnInteraccion)
        {
            return; 
        }

        // 1. Actualizar la dirección de movimiento solo si no está interactuando
        UpdateFacingDirection();

        // 2. Comprobar objeto de frente
        IInteractable interactable = GetInteractableInFront();

        // 3. Intentar interactuar solo si el UITextManager permite interactuar
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && interactable != null)
        {
            if (UITextManager.Instance != null && UITextManager.Instance.PuedeInteractuar)
            {
                interactable.Interact();
            }
        }
    }

    private void UpdateFacingDirection()
    {
        if (Keyboard.current == null) return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;
        else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;

        if (moveX != 0)
        {
            lastFacingDirection = new Vector2(moveX, 0).normalized;
        }
        else if (moveY != 0)
        {
            lastFacingDirection = new Vector2(0, moveY).normalized;
        }
    }

    public IInteractable GetInteractableInFront()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lastFacingDirection, interactDistance, interactableLayer);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<IInteractable>();
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + lastFacingDirection * interactDistance);
    }
}