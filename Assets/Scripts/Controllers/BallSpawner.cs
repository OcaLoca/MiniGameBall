// BallSpawner.cs
using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject ballPrefab;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private float initialDelayMax = 1.0f; // NUOVO: Ritardo massimo casuale all'inizio

    [Header("Launch Settings")]
    [SerializeField] private float minLaunchForce = 400f;
    [SerializeField] private float maxLaunchForce = 600f;
    [SerializeField] private float maxAngleDeviation = 5.0f;

    private Vector3 spawnPoint;


    void Awake()
    {
        spawnPoint = transform.position;
    }

    // RIMOSSI StartSpawning() e SpawnBallsRoutine()

    /// <summary>
    /// Metodo pubblico che esegue un SINGOLO lancio della pallina.
    /// Viene chiamato dal Manager.
    /// </summary>
    public void LaunchBall()
    {
        // ... (Logica di SpawnNewBall spostata qui) ...

        GameObject newBall = Instantiate(ballPrefab, spawnPoint, Quaternion.identity);

        Rigidbody ballRb = newBall.GetComponent<Rigidbody>();

        if (ballRb != null)
        {
            float randomLaunchForce = Random.Range(minLaunchForce, maxLaunchForce);

            Vector3 baseLaunchDirection = transform.forward;

            float horizontalAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);
            float verticalAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);

            Quaternion randomHorizontalRotation = Quaternion.AngleAxis(horizontalAngle, transform.up);
            Quaternion randomVerticalRotation = Quaternion.AngleAxis(verticalAngle, transform.right);

            Vector3 finalLaunchDirection = randomHorizontalRotation * randomVerticalRotation * baseLaunchDirection;

            ballRb.AddForce(finalLaunchDirection * randomLaunchForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("Il Prefab della pallina non ha un componente Rigidbody! Non è possibile applicare la spinta.");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        Gizmos.color = Color.yellow;
        // La linea gialla mostra solo la direzione base (senza deviazione)
        Gizmos.DrawRay(transform.position, transform.forward * 2);
    }
}
