// SpawnersManager.cs
using System;
using System.Collections;
using UnityEngine;

public class SpawnersManager : MonoBehaviour
{
    [Header("Wave Sequence")]
    [Tooltip("Definisci la sequenza di Onda (Waves) del gioco.")]
    [SerializeField] private SpawnWaveData[] waves;

    [Header("Global Settings")]
    [Tooltip("Ritardo generale prima di iniziare la prima onda.")]
    [SerializeField] private float preGameDelay = 2.0f;

    [Tooltip("Frequenza con cui ogni spawner lancia le palline, una volta attivo.")]
    [SerializeField] private float spawnFrequency = 2.0f;


    private void Start()
    {
        StartCoroutine(StartSpawnSequence());
    }

    // ... (SpawnCycleRoutine rimane lo stesso, accetta (spawner, frequency, numberOfBalls)) ...
    IEnumerator SpawnCycleRoutine(BallSpawner spawner, float frequency, int numberOfBalls)
    {
        if (spawner == null) yield break;
        // La logica di lancio limitato rimane invariata
        for (int i = 0; i < numberOfBalls; i++)
        {
            spawner.LaunchBall();
            yield return new WaitForSeconds(frequency);
        }
        Debug.Log($"-> Spawner {spawner.gameObject.name} ha completato il batch.");
    }


    IEnumerator StartSpawnSequence()
    {
        Debug.Log("Inizio la sequenza di Onda in " + preGameDelay.ToString("F1") + " secondi...");
        yield return new WaitForSeconds(preGameDelay);

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("Nessuna Onda (Wave) configurata nello SpawnersManager!");
            yield break;
        }

        // NUOVO: CICLO INFINITO PER RIPETERE L'INTERA SEQUENZA
        while (true)
        {
            Debug.Log("======================================");
            Debug.Log($"Inizio un nuovo Ciclo di Onde ({waves.Length} Onde totali).");

            // Iteriamo su ogni Onda definita nell'Inspector (Wave 1, 2, 3, 4...)
            for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
            {
                SpawnWaveData currentWave = waves[waveIndex];

                Debug.Log($"--- AVVIO ONDA {waveIndex + 1} ---");

                // --- 1. ATTIVAZIONE ---
                // Avvia le Coroutine per ogni Spawner nell'Onda
                foreach (SpawnerConstruct construct in currentWave.spawnersToActivate)
                {
                    if (construct.spawner != null)
                    {
                        StartCoroutine(SpawnCycleRoutine(
                            construct.spawner,
                            spawnFrequency,
                            construct.ballsPerSpawner)
                        );
                    }
                }

                // --- 2. TEMPORIZZAZIONE ---
                // Attendi il tempo definito prima di avviare l'Onda successiva
                Debug.Log($"Attendo {currentWave.waitTimeBeforeNextWave} secondi per la prossima Onda...");
                yield return new WaitForSeconds(currentWave.waitTimeBeforeNextWave);
            }

            Debug.Log("======================================");
            Debug.Log("Ciclo di Onde completato. Riavvio dalla prima Onda.");

            // L'iterazione torna automaticamente all'inizio del 'while(true)'
        }
    }
}

[Serializable]
public class SpawnWaveData
{
    [Tooltip("La lista degli Spawner che si attiveranno contemporaneamente in questa Onda.")]
    public SpawnerConstruct[] spawnersToActivate;

    [Tooltip("Il tempo di attesa dopo l'attivazione di questa Onda, prima di passare alla prossima.")]
    public float waitTimeBeforeNextWave = 5f;
}


// NUOVO: Usiamo una CLASSE invece di una struct
[Serializable]
public struct SpawnerConstruct
{
    public BallSpawner spawner;
    public int ballsPerSpawner;
}