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
    public float slapForce = 30f; 
    public float slapRadius = 2f;
    public LayerMask opponentLayer;
    private float _timeSinceLastSlap = 0f;

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        
        // CONFIGURACIÓN FÍSICA: Usar valores del Inspector
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public override void OnEpisodeBegin()
    {
        // LOG PROFUNDO: Para saber quién nos está teletransportando (Punto 1.1)
        Debug.Log($"<color=orange>🔄 IA REINICIANDO EPISODIO | Pos actual: {transform.position} | Frame: {Time.frameCount}</color>");

        // Reset FULL: Posición segura (Rango -2, 2 World Space), rotación limpia
        transform.position = new Vector3(Random.Range(-2f, 2f), 1.0f, Random.Range(-2f, 2f));
        transform.rotation = Quaternion.identity;
        
        // Reset Físico Absoluto
        if (_rb != null)
        {
            _rb.isKinematic = false;
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

        // NUEVO SISTEMA: Aceleración suave para evitar explosiones físicas
        Vector3 targetVelocity = moveInput * moveSpeed;
        Vector3 currentVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velocityChange = targetVelocity - currentVelocity;
        
        // Aplicar aceleración en lugar de cambio instantáneo
        _rb.AddForce(velocityChange * 2f, ForceMode.Acceleration);

        // SPEED CLAMP: Evita que el agente salga volando al infinito tras un choque
        if (_rb.linearVelocity.magnitude > 20f)
        {
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f);
        }

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
        float distToCenter = Vector3.Distance(transform.position, Vector3.zero); // World space check
        if (distToCenter < 2.5f)
        {
            AddReward(0.001f); // Recompensa aumentada de 0.0001f a 0.001f para supervivencia
        }

        // TAREA 1.1: Penalización por tiempo reducida para evitar "suicidios" prematuros
        AddReward(-0.0002f);

        // DETECTOR DE CAÍDA (OOB): Ahora en FixedUpdate para mayor precisión física (Punto 2.1)
        if (transform.position.y < -0.5f)
        {
            Debug.Log($"<color=red>IA Caída detectada en FixedUpdate a Y={transform.position.y}</color>");
            SetReward(-2.0f);
            EndEpisode();
        }
    }

    private void ExecuteSlap()
    {
        _timeSinceLastSlap = 0f;
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, slapRadius, opponentLayer);

        bool hitOpponent = false;
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                // PROTECCIÓN: Si la dirección es cero, no aplicamos fuerza (Punto 2.2)
                Vector3 slapDir = transform.forward;
                if (slapDir.sqrMagnitude > 0.001f)
                {
                    hitRb.AddForce(slapDir * slapForce, ForceMode.Impulse);
                    hitOpponent = true;
                }
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
        // MOVIDO A FIXED UPDATE: Para evitar conflictos entre FPS y Motor de Físicas

        // TEST: Pulsa K para que la IA dé una bofetada (solo para probar el impulso)
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame) ExecuteSlap();
    }

    // TAREA 3.2: Implementar Heuristic vacío para satisfacer ML-Agents y evitar warnings (Punto 2)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Placeholder vacío
    }
}
