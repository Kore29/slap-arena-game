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
    
    public float slapForce = 50f; // Aumentado para compensar el damping (Punto 3.3)
    public float slapRadius = 0.5f; 
    public float maxSlapDistance = 4f; 
    public LayerMask opponentLayer;

    // EVENTOS PARA HUD
    public System.Action OnSlapHit;

    private float _verticalRotation = 0f;
    private float _lastSlapTime = 0f;
    private const float SlapCooldown = 0.5f;

    // PROPIEDAD PARA HUD
    public float CooldownProgress => Mathf.Clamp01((Time.time - _lastSlapTime) / SlapCooldown);

    // A flag to force local mode even if NetworkManager exists (useful for practice)
    public bool isLocalForce = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        gameObject.tag = "Player";
        
        // Estabilizar física
        if (_rb != null)
        {
            _rb.linearDamping = 0.1f; // Damping bajo para que la gravedad (Y) sea normal
            _rb.angularDamping = 10f;
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        // SEGURIDAD: Si nos hemos caído, dejamos de forzar movimiento (Punto 2)
        if (transform.position.y < -1f) return;

        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        }

        Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
        if (_rb.isKinematic) _rb.isKinematic = false;

        Vector3 targetVelocity = moveDirection * movementSpeed;
        Vector3 currentVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velocityChange = targetVelocity - currentVelocity;

        _rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // FRICCIÓN MANUAL (Punto 2): Como el damping global es 0, frenamos nosotros el eje XZ
        if (moveDirection.magnitude < 0.1f)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.9f, _rb.linearVelocity.y, _rb.linearVelocity.z * 0.9f);
        }    }

    [ServerRpc]
    private void SlapServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        PerformSlap(direction);
    }

    private void PerformSlap(Vector3 direction)
    {
        Debug.Log("Executing Slap logic...");
        
        Vector3 cameraPos = playerCamera != null ? playerCamera.position : transform.position + Vector3.up * 0.8f;
        Vector3 origin = cameraPos + direction * 1f; // El centro de la burbuja un metro adelante
        
        Collider[] hits = Physics.OverlapSphere(origin, slapRadius * 1.5f, opponentLayer | (1 << 0));
        bool hitSuccessful = false;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Debug.Log($"Slap Hit: {hit.name}");
                
                // Aplicar fuerza de empuje
                targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
                hitSuccessful = true;
            }
        }

        if (hitSuccessful)
        {
            // Disparar evento para que la UI se entere
            OnSlapHit?.Invoke();
            
            // Efecto de kick/retroceso
            if (IsOwner) StartCoroutine(ApplySlapKick());
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
