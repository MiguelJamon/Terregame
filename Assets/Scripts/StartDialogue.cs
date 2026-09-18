using System.Collections;
using UnityEngine;

public class StartDialogue : MonoBehaviour
{
    [Header("Configuración de Inicio")]
    [TextArea(2, 5)]
    [SerializeField] private string textoInicial = "Bienvenido a la aventura. Explora el entorno e interactúa con los objetos presionando 'E'.";
    [SerializeField] private float retrasoInicial = 0.5f; // Pequeña pausa antes de mostrar el texto

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(retrasoInicial);

        if (UITextManager.Instance != null)
        {
            UITextManager.Instance.MostrarTexto(textoInicial);
        }
    }
}