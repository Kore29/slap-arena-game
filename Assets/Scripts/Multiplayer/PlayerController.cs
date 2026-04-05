using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody _rb;
    public float slapForce = 15f;
    public float slapRadius = 2f;
    public LayerMask opponentLayer;

    // A flag to force local mode even if NetworkManager exists (useful for practice)
    public bool isLocalForce = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        gameObject.tag = "Player";
    }

    private bool IsNetworkActive => !isLocalForce && NetworkManager.Singleton != null && IsSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Debug.Log($"Network Player Spawned: {OwnerClientId}");
        }
    }

    private void Update()
    {
        // Decision: Should we have control?
        // 1. If Online -> only if IsOwner
        // 2. If Offline -> always (since it's a local object)
        bool hasControl = !IsNetworkActive || IsOwner;
        
        if (!hasControl) return;

        HandleInput();
    }

    private void HandleInput()
    {
        // 1. Mouse Clicks for Slapping
        bool isClicking = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);
        if (isClicking)
        {
            Vector3 direction = transform.forward;
            
            if (IsNetworkActive)
            {
                SlapServerRpc(direction);
            }
            else
            {
                // Local direct execution
                PerformSlap(direction);
            }
        }

        // 2. WASD Movement
        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        }
        
        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;
        if (moveInput.magnitude > 0.1f)
        {
            _rb.MovePosition(_rb.position + moveInput * 5f * Time.deltaTime);
            transform.forward = Vector3.Slerp(transform.forward, moveInput, Time.deltaTime * 10f);
        }
    }

    [ServerRpc]
    private void SlapServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        PerformSlap(direction);
    }

    private void PerformSlap(Vector3 direction)
    {
        Debug.Log("Executing Slap logic...");
        Collider[] hits = Physics.OverlapSphere(transform.position + direction * 1f, slapRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
            }
        }
    }
}
