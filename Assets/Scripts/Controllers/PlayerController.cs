// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private IInputProvider keyboardInterface;

    public float moveSpeed;
    public float sprintMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        keyboardInterface = new KeyboardInputProvider();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float horizontal = keyboardInterface.GetHorizontalInput();
        float currentSpeed = moveSpeed;

        if (keyboardInterface.IsSprintActive())
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 velocity = new Vector3(horizontal * currentSpeed, rb.linearVelocity.y, 0f);
        rb.linearVelocity = velocity;
    }
}
