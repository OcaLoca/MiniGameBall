using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 12f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.01f)
        {
            if (speed < minSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
            else if (speed > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
