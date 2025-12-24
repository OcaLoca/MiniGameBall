// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private IInputProvider keyboardInterface;

    public float moveSpeed;
    public float sprintMultiplier;
    public float maxLeftPosition = -8.9f, maxRightPosition = 8.9f;

    [Header("Attack Settings")] // NUOVE VARIABILI
    [Tooltip("Raggio massimo entro cui l'onda spingerà le palline.")]
    public float knockbackRadius = 5f;
    [Tooltip("Forza applicata alle palline colpite dall'onda d'urto.")]
    public float knockbackForce = 1000f;
    [Tooltip("Tempo di cooldown tra un attacco e l'altro.")]
    public float attackCooldown = 1f;

    private float nextAttackTime = 0f; // Variabile per il cooldown

    [Tooltip("Layer che contiene gli oggetti su cui applicare la spinta (es. le Palline).")]
    public LayerMask ballLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        keyboardInterface = new KeyboardInputProvider();
    }

    private void FixedUpdate()
    {
        Move();
        ApplyPositionLimits();
        Attack();
    }


    void Attack()
    {
        // Controlla se è attivo l'input di attacco E se il cooldown è terminato
        if (keyboardInterface.OnAttack() && Time.time >= nextAttackTime)
        {
            Debug.Log("Attack! Onda energetica generata.");
            nextAttackTime = Time.time + attackCooldown; // Imposta il prossimo tempo di attacco

            ApplyKnockbackWave(); // Chiama la nuova funzione di spinta
        }
    }

    private void ApplyKnockbackWave()
    {
        // 1. Rileva tutti i collider entro il raggio (usando solo il layer delle palline)
        // Usa OverlapSphere per rilevare gli oggetti in una sfera immaginaria
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            knockbackRadius
            // Filtra per colpire solo gli oggetti sul Layer 'ballLayer'
        );

        if (hitColliders.Length > 0)
        {
            Debug.Log($"Colpiti {hitColliders.Length} oggetti.");
        }

        // 2. Itera su ogni oggetto colpito e applica la forza
        foreach (var hitCollider in hitColliders)
        {
            // Tenta di ottenere il Rigidbody della pallina
            Rigidbody ballRb = hitCollider.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                // Calcola la direzione (dal giocatore alla pallina)
                Vector3 directionToBall = (ballRb.position - transform.position).normalized;

                // Ignora la componente Y per mantenere la spinta sul piano (se il gioco è 2D)
                directionToBall.y = 0;
                directionToBall.Normalize();

                // Applica una forza impulsiva radiale
                ballRb.AddForce(directionToBall * knockbackForce, ForceMode.Impulse);
            }
        }

        // OPZIONALE: Qui puoi istanziare effetti particellari (FX) o suoni
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

    /// <summary>
    /// Limita la posizione del Giocatore sull'asse X tra i confini definiti.
    /// </summary>
    private void ApplyPositionLimits()
    {
        // Ottieni la posizione corrente
        Vector3 currentPosition = rb.position;

        // Calcola la nuova posizione X vincolata
        float clampedX = Mathf.Clamp(currentPosition.x, maxLeftPosition, maxRightPosition);

        // Se la posizione è cambiata (è stata limitata)
        if (clampedX != currentPosition.x)
        {
            // Imposta la nuova posizione vincolata
            rb.position = new Vector3(clampedX, currentPosition.y, currentPosition.z);

            // Se si blocca sul confine, azzera la velocità orizzontale per evitare "spinte"
            if (rb.linearVelocity.x != 0f)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);
            }
        }
    }

    /// <summary>
    /// Disegna il raggio dell'onda d'urto nella Scena di Unity (solo nell'Editor).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Controlla per evitare errori se lo script viene chiamato in situazioni strane
        if (transform == null) return;

        // 1. Imposta il colore del Gizmo (ad esempio, Verde chiaro)
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.7f); // R, G, B, Alpha (trasparenza)

        // 2. Disegna una sfera trasparente (wire sphere) attorno alla posizione del giocatore
        // Il raggio è dato dalla variabile pubblica 'knockbackRadius'
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);

        // Se preferisci una sfera piena e trasparente:
        // Gizmos.DrawSphere(transform.position, knockbackRadius); 
    }
}
