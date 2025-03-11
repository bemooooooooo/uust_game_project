using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is not set in CameraController.");
            return;
        }

        Vector3 desiredPosition = player.position + offset;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
