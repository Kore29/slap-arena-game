using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private float movementSpeed = 12f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private Transform playerCamera;
    
    public float slapForce = 15f; 
    public float slapRadius = 0.5f; 
    public float maxSlapDistance = 3f;
    public LayerMask opponentLayer;

    private float _verticalRotation = 0f;
    private float _lastSlapTime = 0f;
    private const float SlapCooldown = 0.5f;

    // A flag to force local mode even if NetworkManager exists (useful for practice)
    public bool isLocalForce = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        gameObject.tag = "Player";
        
        // Estabilizar física (Unity 6 Damping)
        if (_rb != null)
        {
            _rb.linearDamping = 5f;
            _rb.angularDamping = 5f;
            _rb.useGravity = true;
            _rb.isKinematic = false;
        }
    }

    private bool IsNetworkActive => !isLocalForce && NetworkManager.Singleton != null && IsSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Debug.Log($"Network Player Spawned: {OwnerClientId}");
            SetupLocalPlayer();
        }
    }

    private void Start()
    {
        // Si no estamos en red (Práctica local), configuramos el control de todas formas
        if (!IsNetworkActive)
        {
            Debug.Log("Local Practice Player Initialized");
            SetupLocalPlayer();
        }
    }

    private void SetupLocalPlayer()
    {
        // Task 1.3: Bloqueo de cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        HandleLook();
        HandleSlapInput();
    }

    private void FixedUpdate()
    {
        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        HandleMovement();
    }

    private void HandleLook()
    {
        if (Mouse.current == null) return;

        // Task 1.2: Lógica de rotación de ratón (Look)
        float mouseX = Mouse.current.delta.x.ReadValue() * lookSensitivity * 0.1f;
        float mouseY = Mouse.current.delta.y.ReadValue() * lookSensitivity * 0.1f;

        // Rotación horizontal (Cuerpo)
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical (Cámara)
        if (playerCamera != null)
        {
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
            playerCamera.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }
    }

    private void HandleSlapInput()
    {
        // Task 3.2: Sistema de Cooldown
        if (Time.time < _lastSlapTime + SlapCooldown) return;

        bool isClicking = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);
        if (isClicking)
        {
            _lastSlapTime = Time.time;
            
            // Task 3.1: Dirección desde la cámara
            Vector3 direction = playerCamera != null ? playerCamera.forward : transform.forward;
            
            if (IsNetworkActive)
            {
                SlapServerRpc(direction);
            }
            else
            {
                PerformSlap(direction);
            }
        }
    }

    private void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        }

        // Task 2.1: Movimiento relativo a la vista
        Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
        
        // FUERZA FÍSICA (Fix): Si el sistema de red o ML-Agents lo puso en Kinematic, lo desactivamos aquí
        if (_rb.isKinematic) _rb.isKinematic = false;

        // FISICA DE CAIDA (Fix): Usamos linearVelocity para que la gravedad actue en el eje Y
        Vector3 playerVelocity = moveDirection * movementSpeed;
        _rb.linearVelocity = new Vector3(playerVelocity.x, _rb.linearVelocity.y, playerVelocity.z);
    }

    [ServerRpc]
    private void SlapServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        PerformSlap(direction);
    }

    private void PerformSlap(Vector3 direction)
    {
        Debug.Log("Executing Slap logic...");
        
        // Task 3.1: SphereCast desde la cámara (Fix: Offset de origen para no golpearse a uno mismo)
        Vector3 cameraPos = playerCamera != null ? playerCamera.position : transform.position + Vector3.up * 0.8f;
        Vector3 origin = cameraPos + direction * 0.5f; // Empezamos el rayo medio metro adelante
        
        // Detectamos capas de oponente y el default, pero ignoramos nuestro propio collider
        if (Physics.SphereCast(origin, slapRadius * 1.5f, direction, out RaycastHit hit, maxSlapDistance + 1f, opponentLayer | (1 << 0))) 
        {
            if (hit.collider.gameObject == gameObject) return;
            
            Debug.Log($"Slap Hit: {hit.collider.name}");
            
            Rigidbody targetRb = hit.collider.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                // Aplicar fuerza de empuje
                targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
                
                // Efecto de kick/retroceso (Punto 4)
                if (IsOwner) StartCoroutine(ApplySlapKick());
            }
        }
    }

    private System.Collections.IEnumerator ApplySlapKick()
    {
        if (playerCamera == null) yield break;
        
        Vector3 originalPos = playerCamera.localPosition;
        playerCamera.localPosition -= Vector3.forward * 0.15f; // Un poco más de kick
        yield return new WaitForSeconds(0.05f);
        playerCamera.localPosition = originalPos;
    }
}
