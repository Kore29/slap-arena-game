using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody _rb;
    public float slapForce = 15f;
    public float slapRadius = 2f;
    public LayerMask opponentLayer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Tag the player to be easily identified by the ejection zone and ML agents
        gameObject.tag = "Player";
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            Debug.Log($"Local player spawned on network. OwnerId: {OwnerClientId}");
        }
        else
        {
            // If it's not the owner, we still sync physics via NetworkRigidbody,
            // but we shouldn't let local physics interference happen
        }
    }

    private void Update()
    {
        // Only process inputs for the local player
        if (!IsOwner) return;

        // Task 3.2: Detect Mouse Clicks
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Left (0) or Right (1) click implies a slap forward
            Vector3 forwardSlapDir = transform.forward;
            
            // Task 3.3: Request server to apply forces globally
            SlapServerRpc(forwardSlapDir);
        }

        // Basic WASD movement for human control placeholder (for testing)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;
        if (moveInput.magnitude > 0)
        {
            _rb.MovePosition(_rb.position + moveInput * 5f * Time.deltaTime);
            transform.forward = Vector3.Slerp(transform.forward, moveInput, Time.deltaTime * 10f);
        }
    }

    [ServerRpc]
    private void SlapServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        // Execute slap logic on the server
        Debug.Log($"Slap initiated by Owner {rpcParams.Receive.SenderClientId}");

        // Physical cast to detect opponents in front of player
        Collider[] hits = Physics.OverlapSphere(transform.position + direction * 1f, slapRadius);

        foreach (var hit in hits)
        {
            // Make sure we don't slap ourselves
            if (hit.gameObject == gameObject) continue;

            // Apply force to opponent's Rigidbody
            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                // Push them away with physical force
                targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
                Debug.Log($"Opponent hit! Applying force: {direction * slapForce}");
            }
        }
    }
}
