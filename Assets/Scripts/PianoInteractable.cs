using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PianoInteractable : MonoBehaviour, IInteractable
{
    [Header("Mensaje del Piano")]
    [TextArea(2, 5)]
    [SerializeField] private string textoPiano = "Es un piano antiguo. Tocar la tecla Do te trae nostálgicos recuerdos...";
    [SerializeField] private string promptInteraccion = "Presiona E para tocar el piano";

    public void Interact()
    {
        // Llamamos al gestor de UI para que muestre el texto
        if (UITextManager.Instance != null)
        {
            UITextManager.Instance.MostrarTexto(textoPiano);
        }
        else
        {
            Debug.LogWarning("No se encontró el UITextManager en la escena.");
        }
    }

    public string GetInteractPrompt()
    {
        return promptInteraccion;
    }
}