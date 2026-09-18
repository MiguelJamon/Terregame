using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UITextManager : MonoBehaviour
{
    public static UITextManager Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject canvasUI;
    [SerializeField] private TextMeshProUGUI textoUI;

    [Header("Configuración de Texto e Interacción")]
    [SerializeField] private float velocidadEscritura = 0.04f;
    [SerializeField] private float tiempoEsperaTrasCerrar = 1.0f; // Tiempo de espera antes de poder interactuar otra vez

    public bool EstaEnInteraccion { get; private set; } = false;
    public bool PuedeInteractuar { get; private set; } = true;

    private Coroutine tipoEscrituraCoroutine;
    private bool estaEscribiendo = false;
    private string mensajeCompleto = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (canvasUI != null) 
        {
            canvasUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (!EstaEnInteraccion) return;

        if (Keyboard.current != null && 
           (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            if (estaEscribiendo)
            {
                CompletarTexto();
            }
            else
            {
                CerrarUI();
            }
        }
    }

    public void MostrarTexto(string mensaje)
    {
        // Si no se puede interactuar o ya hay un diálogo activo, ignorar la petición
        if (!PuedeInteractuar || EstaEnInteraccion || canvasUI == null || textoUI == null) return;

        mensajeCompleto = mensaje;
        EstaEnInteraccion = true;
        PuedeInteractuar = false; // Bloquea nuevas interacciones

        canvasUI.SetActive(true);

        if (tipoEscrituraCoroutine != null) StopCoroutine(tipoEscrituraCoroutine);
        tipoEscrituraCoroutine = StartCoroutine(EscribirTexto());
    }

    private IEnumerator EscribirTexto()
    {
        estaEscribiendo = true;
        textoUI.text = "";

        foreach (char letra in mensajeCompleto.ToCharArray())
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        estaEscribiendo = false;
    }

    private void CompletarTexto()
    {
        if (tipoEscrituraCoroutine != null) StopCoroutine(tipoEscrituraCoroutine);
        textoUI.text = mensajeCompleto;
        estaEscribiendo = false;
    }

    private void CerrarUI()
    {
        if (tipoEscrituraCoroutine != null) StopCoroutine(tipoEscrituraCoroutine);
        canvasUI.SetActive(false);
        estaEscribiendo = false;
        EstaEnInteraccion = false;

        // Inicia el temporizador de 1 segundo para reactivar la interacción
        StartCoroutine(ColectaTiempoEspera());
    }

    private IEnumerator ColectaTiempoEspera()
    {
        yield return new WaitForSeconds(tiempoEsperaTrasCerrar);
        PuedeInteractuar = true; // Reactiva la capacidad de interactuar
    }
}