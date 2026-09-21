using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool active = true;
    [SerializeField] private float intensity = 0.3f;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float frequency = 25f;
    [SerializeField] private float smoothness = 0.1f;
    [SerializeField] private bool increaseOverTime = true;
    [SerializeField] private bool decreaseOverTime = true;

    [Header("Referencias")]
    [SerializeField] private CameraFollow cameraFollow;

    private bool isShaking;
    private float shakeTimer;
    private float shakeIntensity;
    private Vector3 shakeOffset;

    public bool Active
    {
        get => active;
        set => active = value;
    }

    private void Awake()
    {
        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollow>();
        }
    }

    private void Update()
    {
        if (!active || !isShaking)
        {
            StopShake(false);
            return;
        }

        shakeTimer -= Time.deltaTime;

        float progress = Mathf.Clamp01(1f - (shakeTimer / Mathf.Max(duration, 0.01f)));
        float currentStrength = shakeIntensity;

        if (increaseOverTime && !decreaseOverTime)
        {
            currentStrength = Mathf.Lerp(0f, shakeIntensity, Mathf.SmoothStep(0f, 1f, progress));
        }
        else if (!increaseOverTime && decreaseOverTime)
        {
            currentStrength = Mathf.Lerp(shakeIntensity, 0f, Mathf.SmoothStep(0f, 1f, progress));
        }
        else if (increaseOverTime && decreaseOverTime)
        {
            currentStrength = shakeIntensity * Mathf.SmoothStep(0f, 1f, progress);
        }

        float x = (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f) * 2f * currentStrength;
        float y = (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f) * 2f * currentStrength;
        shakeOffset = new Vector3(x, y, 0f);

        if (cameraFollow != null)
        {
            cameraFollow.ShakeOffset = shakeOffset;
        }
        else
        {
            transform.localPosition += shakeOffset * smoothness;
        }

        if (shakeTimer <= 0f)
        {
            StopShake(false);
        }
    }

    public void StartShake(float customIntensity = -1f, float customDuration = -1f)
    {
        if (!active)
        {
            return;
        }

        shakeIntensity = customIntensity > 0f ? customIntensity : intensity;

        if (customDuration > 0f)
        {
            duration = customDuration;
        }

        shakeTimer = duration;
        isShaking = true;

        if (cameraFollow != null)
        {
            cameraFollow.ShakeOffset = Vector3.zero;
        }
    }

    public void StopShake(bool resetOffset = true)
    {
        isShaking = false;
        shakeTimer = 0f;

        if (resetOffset)
        {
            shakeOffset = Vector3.zero;
        }

        if (cameraFollow != null)
        {
            cameraFollow.ShakeOffset = Vector3.zero;
        }
    }
}
