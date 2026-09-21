using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EarthquakeManager : MonoBehaviour
{
    public static EarthquakeManager Instance { get; private set; }

    [Header("Inicio del terremoto")]
    [SerializeField] private float earthquakeStartDelay = 8f;
    [SerializeField] private float earthquakeDuration = 12f;
    [SerializeField] private bool startOnAwake = true;

    [Header("Mensaje del terremoto")]
    [SerializeField] private ScreenMessageController messageController;
    [SerializeField] private ScreenMessageController.MessageSettings earthquakeMessageSettings = new ScreenMessageController.MessageSettings
    {
        text = "Comenzó a temblar",
        displayDuration = 2f,
        fontSize = 42f,
        textColor = Color.white,
        backgroundColor = new Color(0f, 0f, 0f, 0.55f),
        anchoredPosition = new Vector2(0f, 120f),
        transparency = 1f,
        freezeGameplay = true
    };

    [Header("Temblor")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private bool triggerCameraShake = true;

    [Header("Objetivos")]
    [SerializeField] private ObjectiveManager objectiveManager;

    private bool earthquakeStarted;
    private bool earthquakeActive;
    private bool objectivesShownAfterEarthquakeMessage;
    private bool failStateTriggered;
    private bool objectiveVictoryHandled;
    private Coroutine earthquakeEndRoutine;

    public bool IsEarthquakeActive => earthquakeActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (messageController == null)
        {
            messageController = FindAnyObjectByType<ScreenMessageController>();
        }

        if (cameraShake == null)
        {
            cameraShake = FindAnyObjectByType<CameraShake>();
        }

        if (objectiveManager == null)
        {
            objectiveManager = FindAnyObjectByType<ObjectiveManager>();
        }
    }

    private void Start()
    {
        if (startOnAwake)
        {
            StartEarthquakeSequence();
        }
    }

    public void StartEarthquakeSequence()
    {
        if (earthquakeStarted)
        {
            return;
        }

        earthquakeStarted = true;
        StartCoroutine(CountdownToEarthquake());
    }

    private IEnumerator CountdownToEarthquake()
    {
        yield return new WaitForSeconds(earthquakeStartDelay);
        TriggerEarthquake();
    }

    public void TriggerEarthquake()
    {
        earthquakeActive = true;
        objectivesShownAfterEarthquakeMessage = false;

        if (messageController != null)
        {
            if (!messageController.gameObject.activeSelf)
            {
                messageController.gameObject.SetActive(true);
            }

            messageController.MessageFinished -= OnEarthquakeMessageFinished;
            messageController.MessageFinished += OnEarthquakeMessageFinished;
            messageController.ShowMessage(earthquakeMessageSettings);
        }

        if (objectiveManager != null)
        {
            objectiveManager.SetObjectiveUIVisible(false);
            objectiveManager.SetCurrentObjective("Recolecta los objetos necesarios y llega a un lugar seguro.");
        }

        if (triggerCameraShake && cameraShake != null)
        {
            cameraShake.StartShake();
        }
    }

    private void OnEarthquakeMessageFinished()
    {
        if (objectivesShownAfterEarthquakeMessage)
        {
            return;
        }

        objectivesShownAfterEarthquakeMessage = true;

        if (messageController != null)
        {
            messageController.MessageFinished -= OnEarthquakeMessageFinished;
        }

        if (objectiveManager != null)
        {
            objectiveManager.SetObjectiveUIVisible(true);
        }

        if (earthquakeEndRoutine != null)
        {
            StopCoroutine(earthquakeEndRoutine);
        }

        earthquakeEndRoutine = StartCoroutine(EarthquakeFailureCheckRoutine());
    }

    private IEnumerator EarthquakeFailureCheckRoutine()
    {
        yield return new WaitForSecondsRealtime(earthquakeDuration);

        if (objectiveVictoryHandled)
        {
            yield break;
        }

        if (objectiveManager != null && objectiveManager.ObjectiveCompleted)
        {
            BeginObjectiveVictoryCleanup();
            yield break;
        }

        if (objectiveManager != null)
        {
            TriggerFailure();
        }
    }

    public void BeginObjectiveVictoryCleanup(float delayAfterCompletionMessage = 1.5f)
    {
        if (objectiveVictoryHandled)
        {
            return;
        }

        objectiveVictoryHandled = true;
        StartCoroutine(EndVictoryAfterDelay(delayAfterCompletionMessage));
    }

    private IEnumerator EndVictoryAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (cameraShake != null)
        {
            cameraShake.StopShake(true);
        }

        if (messageController != null)
        {
            messageController.HideMessage();
        }

        if (objectiveManager != null)
        {
            objectiveManager.SetObjectiveUIVisible(false);
        }

        earthquakeActive = false;
    }

    public void TriggerFailureFromHealth()
    {
        TriggerFailure();
    }

    private void TriggerFailure()
    {
        if (failStateTriggered)
        {
            return;
        }

        failStateTriggered = true;

        DebrisSpawner[] debrisSpawners = FindObjectsByType<DebrisSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in debrisSpawners)
        {
            if (spawner != null)
            {
                spawner.SpawnEnabled = false;
            }
        }

        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetDefeatedPose();
        }

        if (objectiveManager != null)
        {
            objectiveManager.SetObjectiveUIVisible(false);
        }

        if (messageController != null)
        {
            messageController.HideMessage();
            messageController.ShowFailureScreen();
        }

        Time.timeScale = 0f;
        StartCoroutine(RestartFromBeginning());
    }

    private IEnumerator RestartFromBeginning()
    {
        yield return new WaitForSecondsRealtime(1.8f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
