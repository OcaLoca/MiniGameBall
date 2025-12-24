// BotController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BotController : MonoBehaviour
{
    public enum MovementAxis { X, Z }

    private Rigidbody rb;
    private Transform currentTargetBall;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    [Tooltip("Moltiplicatore di velocità quando la pallina è lontana.")]
    public float speedMultiplier = 1.5f;
    public float minPosition = -8.9f, maxPosition = 8.9f;
    public MovementAxis movementAxis = MovementAxis.X;

    [Header("Attack Settings")]
    public float knockbackRadius = 5f;
    public float knockbackForce = 1000f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;
    public LayerMask ballLayer;
    public Tag ballTag = Tag.DefaultBall;

    [Header("Bot AI")]
    public float ballSearchRadius = 15f;
    public float minReachDistance = 0.1f;
    public float autoAttackRange = 3f;
    [Tooltip("Distanza oltre la quale applicare il moltiplicatore di velocità.")]
    public float farDistanceThreshold = 5f;
    [Tooltip("Altezza massima rispetto al bot per considerare una pallina (ignora quelle che cadono dall'alto).")]
    public float maxBallHeight = 2f;
    [Tooltip("TEST: Se attivo, attacca sempre ogni frame (ignora cooldown e controlli).")]
    public bool testAttackAlways = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        DecideMovement();
        Attack();
    }

    private void DecideMovement()
    {
        // Cerca palline nel raggio
        Transform nearestBall = FindNearestBall();

        if (nearestBall != null)
        {
            // C'è una pallina: muoviti verso di essa
            MoveTowardsTarget(nearestBall);
            currentTargetBall = nearestBall;
        }
        else
        {
            // Nessuna pallina: torna al centro (0)
            MoveTowardsCenter();
            currentTargetBall = null;
        }
    }

    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private string GetTagString()
    {
        return ballTag.ToString();
    }

    private Transform FindNearestBall()
    {
        Collider[] colliders = (ballLayer.value == 0)
            ? Physics.OverlapSphere(transform.position, ballSearchRadius)
            : Physics.OverlapSphere(transform.position, ballSearchRadius, ballLayer);

        Transform nearestBall = null;
        float nearestDistance = float.MaxValue;
        Vector3 botPos = transform.position;

        foreach (var col in colliders)
        {
            if (col.transform == transform ||
                col.GetComponent<BotController>() != null ||
                col.GetComponent<PlayerController>() != null)
                continue;

            bool isBall = col.CompareTag(GetTagString());

            if (isBall)
            {
                Vector3 ballPos = col.transform.position;
                
                // Ignora palline troppo in alto (che stanno cadendo)
                if (ballPos.y > botPos.y + maxBallHeight)
                    continue;
                
                float distanceXZ = DistanceXZ(botPos, ballPos);
                
                if (distanceXZ <= ballSearchRadius)
                {
                    float ballCoordinate = (movementAxis == MovementAxis.X) ? ballPos.x : ballPos.z;
                    if (ballCoordinate >= minPosition && ballCoordinate <= maxPosition && distanceXZ < nearestDistance)
                    {
                        nearestDistance = distanceXZ;
                        nearestBall = col.transform;
                    }
                }
            }
        }

        return nearestBall;
    }

    private void MoveTowardsTarget(Transform target)
    {
        float targetCoord = (movementAxis == MovementAxis.X) ? target.position.x : target.position.z;
        float currentCoord = (movementAxis == MovementAxis.X) ? transform.position.x : transform.position.z;
        
        float distance = Mathf.Abs(targetCoord - currentCoord);
        
        if (distance > minReachDistance)
        {
            float direction = Mathf.Sign(targetCoord - currentCoord);
            float currentSpeed = moveSpeed;
            
            // Calcola se il bot riuscirà a intercettare la pallina
            Rigidbody ballRb = target.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                // Calcola la velocità della pallina sull'asse di movimento del bot
                float ballVelocity = (movementAxis == MovementAxis.X) 
                    ? ballRb.linearVelocity.x 
                    : ballRb.linearVelocity.z;
                
                // Se la pallina si sta muovendo verso il bot (direzione opposta)
                bool ballMovingTowardsBot = (ballVelocity < 0 && direction > 0) || (ballVelocity > 0 && direction < 0);
                
                if (ballMovingTowardsBot && Mathf.Abs(ballVelocity) > 0.1f)
                {
                    // Calcola il tempo che ci vuole alla pallina per raggiungere la posizione del bot
                    float timeForBallToReach = distance / Mathf.Abs(ballVelocity);
                    
                    // Calcola il tempo che ci vuole al bot per raggiungere la posizione di intercettazione
                    float timeForBotToReach = distance / moveSpeed;
                    
                    // Se la pallina arriva prima, accelera!
                    if (timeForBallToReach < timeForBotToReach)
                    {
                        currentSpeed *= speedMultiplier;
                    }
                }
            }
            
            MoveBot(direction, currentSpeed);
        }
        else
        {
            MoveBot(0f);
        }
    }

    private void MoveTowardsCenter()
    {
        float currentCoord = (movementAxis == MovementAxis.X) ? transform.position.x : transform.position.z;
        float distanceToCenter = Mathf.Abs(currentCoord);

        if (distanceToCenter > minReachDistance)
        {
            float direction = Mathf.Sign(0f - currentCoord);
            MoveBot(direction);
        }
        else
        {
            MoveBot(0f);
        }
    }

    private void MoveBot(float inputDirection, float speed = -1f)
    {
        if (speed < 0f) speed = moveSpeed;
        
        if (movementAxis == MovementAxis.X)
        {
            rb.linearVelocity = new Vector3(inputDirection * speed, rb.linearVelocity.y, 0f);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, inputDirection * speed);
        }
    }

    void Attack()
    {
        // TEST: Se attivo, attacca sempre
        if (testAttackAlways)
        {
            Debug.Log($"[Bot {gameObject.name}] ⚡ TEST ATTACK - Attacco forzato!");
            ApplyKnockbackWave();
            return;
        }

        // Se l'attacco non è ricaricato, esci
        if (Time.time < nextAttackTime)
            return;

        // Cerca tutti i collider nel raggio (senza filtro layer se non configurato)
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, autoAttackRange);

        // Se c'è una pallina nel raggio, attacca!
        foreach (var col in nearbyColliders)
        {
            // Escludi se stesso, bot, player
            if (col.transform == transform ||
                col.GetComponent<BotController>() != null ||
                col.GetComponent<PlayerController>() != null)
                continue;

            // Controlla solo il tag
            if (col.CompareTag(GetTagString()))
            {
                float distanceXZ = DistanceXZ(transform.position, col.transform.position);

                if (distanceXZ <= autoAttackRange)
                {
                    nextAttackTime = Time.time + attackCooldown;
                    ApplyKnockbackWave();
                    currentTargetBall = null;
                    return; // Attacca solo una volta
                }
            }
        }
    }

    private void ApplyKnockbackWave()
    {
        // Cerca tutti i collider (senza filtro layer se non configurato)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, knockbackRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Escludi se stesso, bot, player
            if (hitCollider.transform == transform ||
                hitCollider.GetComponent<BotController>() != null ||
                hitCollider.GetComponent<PlayerController>() != null)
                continue;

            // Controlla solo il tag
            if (!hitCollider.CompareTag(GetTagString()))
                continue;

            Rigidbody ballRb = hitCollider.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                Vector3 directionToBall = (ballRb.position - transform.position);
                directionToBall.y = 0;
                directionToBall.Normalize();

                ballRb.AddForce(directionToBall * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
        
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, ballSearchRadius);
    }
}

public enum Tag
{
    DefaultBall
}