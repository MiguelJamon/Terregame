
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform jugador;

    void LateUpdate()
    {
        if (jugador != null)
        {
            transform.position = new Vector3(
                jugador.position.x,
                jugador.position.y,
                transform.position.z
            );
        }
    }
}