public interface IInteractable
{
    // Método que ejecutarán todos los objetos con los que se pueda interactuar
    void Interact();
    
    // Texto descriptivo opcional para mostrar en pantalla (ej: "Presiona E para hablar")
    string GetInteractPrompt();
}