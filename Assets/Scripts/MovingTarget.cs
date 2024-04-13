using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    public float speed = 1.5f;
    public float maxDistance = 4.0f;

    private float startPositionX;

    void Start()
    {
        startPositionX = transform.position.x;
    }

    void FixedUpdate()
    {
        float newPositionX = startPositionX + Mathf.Sin(Time.time * speed) * maxDistance;
        transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);
    }
}
