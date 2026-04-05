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
    
    public float moveSpeed = 15f;
    public float slapForce = 15f;
    public float slapRadius = 2f;
    private float _timeSinceLastSlap = 0f;

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        
        // FORZAR CONFIGURACIÓN FÍSICA (Para evitar errores de Kinematic)
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        // Si por algún motivo el nombre está mal en el inspector, 
        // ML-Agents a veces no deja cambiarlo por código fácilmente, 
        // pero vamos a asegurarnos de que el Rigidbody esté listo.
    }

    public override void OnEpisodeBegin()
    {
        // Reset position on start or if they fall off the arena
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f));
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        // Also reset the opponent for a fresh scenario
        if (targetOpponent != null)
        {
            targetOpponent.localPosition = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f));
            targetOpponent.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
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
        // SEGURIDAD: Evitar que el juego explote si el Inspector está mal configurado
        if (actions.ContinuousActions.Length < 2)
        {
            Debug.LogError($"¡OJO! Tu agente ({gameObject.name}) tiene {actions.ContinuousActions.Length} Acciones Continuas, pero el código necesita 2. Revisa el componente Behavior Parameters en el PREFAB.");
            return;
        }

        if (actions.DiscreteActions.Length < 1)
        {
            Debug.LogError($"¡OJO! Tu agente ({gameObject.name}) tiene 0 Acciones Discretas, pero el código necesita 1. Revisa el componente Behavior Parameters en el PREFAB.");
            return;
        }

        // 5.2 Define agent actions (movement forces)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;
        _rb.AddForce(moveInput * moveSpeed, ForceMode.Force);

        // Face movement direction
        if (moveInput.magnitude > 0.1f)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveInput, Time.fixedDeltaTime * 5f);
        }

        // Action: discrete slap (0 = do nothing, 1 = slap)
        int slapAction = actions.DiscreteActions[0];
        _timeSinceLastSlap += Time.fixedDeltaTime;

        if (slapAction == 1 && _timeSinceLastSlap >= 0.5f)
        {
            ExecuteSlap();
        }

        // General incentive penalty per step to encourage the agent to act fast
        AddReward(-0.001f);
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
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Debug override to let a human control the agent for testing
        var continuousActions = actionsOut.ContinuousActions;
        
        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        }
        
        if (continuousActions.Length >= 2)
        {
            continuousActions[0] = moveX;
            continuousActions[1] = moveZ;
        }

        var discreteActions = actionsOut.DiscreteActions;
        bool isClicking = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);
        
        if (discreteActions.Length >= 1)
        {
            discreteActions[0] = isClicking ? 1 : 0;
        }
    }
}
