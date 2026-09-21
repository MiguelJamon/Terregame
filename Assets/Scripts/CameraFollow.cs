
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = Vector3.zero;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    [Header("Temblor")]
    [SerializeField] private Vector3 shakeOffset;

    public Transform jugador;

    public Vector3 ShakeOffset
    {
        get => shakeOffset;
        set => shakeOffset = value;
    }

    private void Awake()
    {
        if (target == null && jugador != null)
        {
            target = jugador;
        }
    }

    private void LateUpdate()
    {
        Transform trackedTarget = target != null ? target : jugador;

        if (trackedTarget != null)
        {
            Vector3 desiredPosition = trackedTarget.position + followOffset + shakeOffset;

            float x = followX ? desiredPosition.x : transform.position.x;
            float y = followY ? desiredPosition.y : transform.position.y;

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}