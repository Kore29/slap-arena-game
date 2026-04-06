using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAgent : Agent
{
    private Rigidbody _rb;
    public Transform targetOpponent;
    
    public float moveSpeed = 5f;
    public float slapForce = 50f;
    public float slapRadius = 2f;
    private float _timeSinceLastSlap = 0f;

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        
        // FORZAR CONFIGURACIÓN FÍSICA (Punto 1.3)
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.linearDamping = 7f; // Damping sincronizado con el jugador
            _rb.angularDamping = 10f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset ONLY the agent's position to a safe spot on the platform
        transform.localPosition = new Vector3(Random.Range(-3f, 3f), 1.1f, Random.Range(-3f, 3f));
        
        // Fix: Solo reseteamos velocidad si el cuerpo es físico en este micro-segundo
        if (_rb != null && !_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 5.1 Define agent observations
        // Observe own position and velocity (6 values)
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(_rb.linearVelocity);

        // Observe opponent's position and distance to them (4 values)
        if (targetOpponent != null)
        {
            sensor.AddObservation(targetOpponent.localPosition);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, targetOpponent.localPosition));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // LOG DE CONTROL: Para saber si la IA está recibiendo señales
        if (Time.frameCount % 50 == 0) Debug.Log("<color=cyan>🤖 IA viva y recibiendo órdenes...</color>");

        if (actions.ContinuousActions.Length < 2) return;

        // 5.2 Define agent actions (movement forces)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;
        
        if (_rb.isKinematic) _rb.isKinematic = false;

        // NUEVO SISTEMA (Punto 2.2): Sincronizado con el jugador
        Vector3 targetVelocity = moveInput * moveSpeed;

        // PARADA POR PROXIMIDAD (Punto 2.3): Para que la IA no intente "atravesar" al jugador
        if (targetOpponent != null)
        {
            float distance = Vector3.Distance(transform.position, targetOpponent.position);
            if (distance < 1.5f) // Si está muy cerca, deja de forzar movimiento hacia adelante
            {
                targetVelocity = Vector3.zero;
            }
        }

        Vector3 currentVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velocityChange = targetVelocity - currentVelocity;
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // FRICCIÓN MANUAL (Punto 2): Como el damping global es 0, frenamos nosotros el eje XZ
        if (moveInput.magnitude < 0.1f)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.9f, _rb.linearVelocity.y, _rb.linearVelocity.z * 0.9f);
        }

        // Face movement direction
        if (moveInput.magnitude > 0.1f)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveInput, Time.fixedDeltaTime * 5f);
        }

        // Action: discrete slap
        int slapAction = actions.DiscreteActions[0];
        _timeSinceLastSlap += Time.fixedDeltaTime;

        if (slapAction == 1 && _timeSinceLastSlap >= 0.5f)
        {
            ExecuteSlap();
        }

        // TAREA 1.2: Recompensa de proximidad (fomentar acercarse al combate)
        if (targetOpponent != null)
        {
            float dist = Vector3.Distance(transform.position, targetOpponent.position);
            if (dist < 3.0f)
            {
                AddReward(0.0005f);
            }
        }

        // TAREA 1.3: Recompensa de zona segura (fomentar no irse a los bordes)
        float distToCenter = Vector3.Distance(transform.localPosition, Vector3.zero);
        if (distToCenter < 2.5f)
        {
            AddReward(0.0001f);
        }

        // TAREA 1.1: Penalización por tiempo reducida para evitar "suicidios" prematuros
        AddReward(-0.0002f);
    }

    private void ExecuteSlap()
    {
        _timeSinceLastSlap = 0f;
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, slapRadius);

        bool hitOpponent = false;
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null && hit.CompareTag("Player"))
            {
                hitRb.AddForce(transform.forward * slapForce, ForceMode.Impulse);
                hitOpponent = true;
            }
        }

        // 5.3 Implement positive reward for hitting opponent
        if (hitOpponent)
        {
            AddReward(1.0f);
        }
    }

    private void Update()
    {
        // 5.3 Implement extreme negative reward for falling off bounds
        if (transform.localPosition.y < -3f)
        {
            SetReward(-2.0f);
            EndEpisode();
        }

        // TEST: Pulsa K para que la IA dé una bofetada (solo para probar el impulso)
        if (Input.GetKeyDown(KeyCode.K)) ExecuteSlap();
    }

    // TAREA 3.2: Implementar Heuristic vacío para satisfacer ML-Agents y evitar warnings (Punto 2)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Placeholder vacío
    }
}
