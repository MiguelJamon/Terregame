using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena principal de juego a cargar")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    [Header("Efecto de Transición y Difuminado")]
    [Tooltip("Cámara principal para el efecto suave")]
    [SerializeField] private Camera menuCamera;

    [Tooltip("CanvasGroup de la interfaz para desvanecerla")]
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Tooltip("Imagen para transición de fundido suave")]
    [SerializeField] private Image fadeOverlay;

    [Tooltip("Duración de la transición")]
    [SerializeField] private float transitionDuration = 1.0f;

    [Header("Paneles de UI")]
    [Tooltip("Panel principal del menú con los botones")]
    [SerializeField] private GameObject mainButtonsPanel;
    
    [Tooltip("Panel de controles e instrucciones")]
    [SerializeField] private GameObject controlsPanel;

    [Header("Botones")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button closeControlsButton;
    [SerializeField] private Button quitButton;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(OnControlsClicked);
        }

        if (closeControlsButton != null)
        {
            closeControlsButton.onClick.AddListener(OnCloseControlsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        if (mainButtonsPanel != null)
        {
            mainButtonsPanel.SetActive(true);
        }

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.raycastTarget = false;
        }
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (controlsButton != null) controlsButton.onClick.RemoveListener(OnControlsClicked);
        if (closeControlsButton != null) closeControlsButton.onClick.RemoveListener(OnCloseControlsClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    /// <summary>
    /// Inicia la secuencia de transición con difuminado suave al pulsar JUGAR.
    /// </summary>
    public void OnPlayClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(PlayTransitionSequence());
    }

    private IEnumerator PlayTransitionSequence()
    {
        isTransitioning = true;
        Debug.Log("[MainMenu] Iniciando transición suave a " + gameplaySceneName);

        // 1. Bloquear interacción en la UI
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        if (menuCamera == null)
        {
            menuCamera = Camera.main;
        }

        float initialOrthoSize = menuCamera != null ? menuCamera.orthographicSize : 5f;
        float targetOrthoSize = initialOrthoSize * 1.15f; // Suave retroceso de alejamiento
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Desvanecer UI suavemente
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = Mathf.Clamp01(1f - smoothT);
            }

            // Suave alejamiento de cámara
            if (menuCamera != null)
            {
                menuCamera.orthographicSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, smoothT);
            }

            // Fundido suave a negro
            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(0f, 0f, 0f, smoothT);
            }

            yield return null;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.color = Color.black;
        }

        yield return new WaitForSeconds(0.05f);

        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("[MainMenu] No se ha especificado el nombre de la escena de juego.");
        }
    }

    /// <summary>
    /// Muestra el panel de controles.
    /// </summary>
    public void OnControlsClicked()
    {
        if (isTransitioning) return;
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Cierra el panel de controles.
    /// </summary>
    public void OnCloseControlsClicked()
    {
        if (isTransitioning) return;
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Sale del videojuego (tanto en Build como en Unity Editor).
    /// </summary>
    public void OnQuitClicked()
    {
        if (isTransitioning) return;
        Debug.Log("[MainMenu] Saliendo del juego...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
