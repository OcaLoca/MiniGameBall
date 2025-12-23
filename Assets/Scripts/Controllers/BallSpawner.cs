using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject ballPrefab;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnInterval = 2.0f;

    // RIMOSSA: [SerializeField] private float spawnDistanceX = 5.0f; // Non serve più

    [Header("Launch Settings")]
    [SerializeField] private float launchForce = 500f;

    private Vector3 spawnPoint;

    void Start()
    {
        // La posizione dello Spawner è il punto di spawn fisso
        spawnPoint = transform.position;

        StartCoroutine(SpawnBallsRoutine());
    }

    /// <summary>
    /// Coroutine che genera una pallina ogni 'spawnInterval' secondi.
    /// </summary>
    IEnumerator SpawnBallsRoutine()
    {
        while (true)
        {
            SpawnNewBall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Logica per istanziare una singola pallina nel punto esatto dello Spawner.
    /// </summary>
    void SpawnNewBall()
    {
        // 1. Istanzia il Prefab nel punto di spawn fisso
        // RIMOSSO: il calcolo di randomPosition.
        GameObject newBall = Instantiate(ballPrefab, spawnPoint, Quaternion.identity);

        // 2. Ottieni e applica la forza
        Rigidbody ballRb = newBall.GetComponent<Rigidbody>();

        if (ballRb != null)
        {
            // La direzione "avanti" dell'oggetto spawner è data da transform.forward
            Vector3 launchDirection = transform.forward;

            // Applica la forza nella direzione dello Spawner
            ballRb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("Il Prefab della pallina non ha un componente Rigidbody! Non è possibile applicare la spinta.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Disegna un cubo per mostrare il punto esatto di spawn
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        // Disegna una linea per mostrare la direzione di lancio
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 2);
    }
}