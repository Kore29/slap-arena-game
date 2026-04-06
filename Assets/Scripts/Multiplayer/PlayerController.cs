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
    
    public float slapForce = 40f; 
    public float slapRadius = 2.0f; 
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
        
        // CONFIGURACIÓN FÍSICA: Sincronizada con el Inspector
        if (_rb != null)
        {
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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

        // Se ha movido a FixedUpdate para evitar conflictos de teletransporte (Punto 2.2)
    }

    private void Respawn()
    {
        Debug.Log($"<color=cyan>🔁 Jugador Respawneando desde {transform.position} | Frame: {Time.frameCount}</color>");
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        if (GameManager.Instance != null && GameManager.Instance.playerSpawnPoint != null)
        {
            transform.position = GameManager.Instance.playerSpawnPoint.position;
        }
        else
        {
            transform.position = new Vector3(0, 2f, 0); // Failsafe
        }
    }

    private void FixedUpdate()
    {
        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        // AUTO-RESPAWN: Ahora en FixedUpdate para mayor precisión física (Punto 2.2)
        if (transform.position.y < -0.5f)
        {
            Respawn();
        }

        HandleMovement();

        // SPEED CLAMP: Limitar velocidad máxima para evitar eyecciones por colisión
        if (_rb.linearVelocity.magnitude > 25f)
        {
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 25f);
        }
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
        // SEGURIDAD: Si nos hemos caído, dejamos de forzar movimiento
        if (transform.position.y < -0.5f) return;

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

        // ACELERACIÓN: Usar modo Acceleration para respetar la masa y suavizar impactos
        _rb.AddForce(velocityChange * 3f, ForceMode.Acceleration);

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
        Vector3 origin = cameraPos + direction; // El centro de la burbuja un metro adelante
        
        // RESTRICCIÓN: Solo buscar en opponentLayer (Eliminada la capa Default para evitar bugs)
        Collider[] hits = Physics.OverlapSphere(origin, slapRadius, opponentLayer);
        bool hitSuccessful = false;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Debug.Log($"<color=yellow>HUBO GOLPE!: {hit.name} | Mass: {targetRb.mass} | Kinematic: {targetRb.isKinematic}</color>");
                Debug.Log($"Fuerza: {slapForce} | Direccion: {direction}");
                
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
