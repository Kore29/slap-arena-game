using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;
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


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
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

    private bool IsNetworkActive => NetworkManager.Singleton != null && IsSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Debug.Log($"Network Player Spawned: {OwnerClientId}");
            SetupLocalPlayer();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            // ACTIVAR CÁMARA DE MUERTE (Espectador)
            ActivateDeathCamera();
        }
        base.OnNetworkDespawn();
    }

    private void ActivateDeathCamera()
    {
        Debug.Log("<color=red>💀 Local Player eliminated! Switching to Spectator Camera.</color>");
        
        // Crear una cámara de emergencia si no hay una libre
        GameObject deathCamObj = new GameObject("SpectatorCamera");
        Camera cam = deathCamObj.AddComponent<Camera>();
        deathCamObj.tag = "MainCamera"; // Taggear como MainCamera para que WorldNameTag lo encuentre
        deathCamObj.AddComponent<AudioListener>();

        // Posicionar más cerca (Bajamos de 35 a 20 de altura)
        deathCamObj.transform.position = new Vector3(0, 18, -15);
        deathCamObj.transform.rotation = Quaternion.Euler(45, 0, 0);
        
        // MOSTRAR MENÚ DE DERROTA INMEDIATO
        MatchResultsController resultsUI = Object.FindAnyObjectByType<MatchResultsController>();
        if (resultsUI != null)
        {
            resultsUI.ShowEliminatedEarly();
        }

        // Desbloquear cursor para que puedan ver resultados luego
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void Start()
    {
    }

    private void SetupLocalPlayer()
    {
        // Solo bloquear el cursor si estamos jugando (Tarea 1.3)
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            Debug.Log("Cursor locked for gameplay.");
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Debug.Log("Cursor unlocked for UI/Lobby.");
        }

        // AUTO-INICIALIZAR HUD (Punto 2.2)
        GameHUD hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null)
        {
            hud.Initialize(this);
            Debug.Log("<color=green>HUD Initialized by PlayerController</color>");
        }
    }

    private void Update()
    {
        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        HandleLook();
        HandleSlapInput();

        // Se ha movido a FixedUpdate para evitar conflictos de teletransporte (Punto 2.2)
    }
    private void FixedUpdate()
    {
        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        // Enviar velocidad al Animator (Task 3.1)
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            // Usamos .velocity para máxima compatibilidad (Task 3.1)
            float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
            _animator.SetFloat("Speed", horizontalSpeed / movementSpeed);
            _animator.SetBool("IsFalling", transform.position.y < -0.5f);
        }
        else {
            Debug.LogWarning("ANIM ERROR: No se encuentra el componente Animator en el Player!");
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
        if (transform.position.y < -10.0f) return;

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
        if (transform.position.y < -10.0f) return; // No golpear si estamos cayendo
        
        // Lanzar animación (Task 3.1)
        if (_animator != null)
        {
            _animator.SetTrigger("Slap");
        }
        
        Debug.Log("Executing Slap logic...");
        
        Vector3 cameraPos = playerCamera != null ? playerCamera.position : transform.position + Vector3.up * 0.8f;
        Vector3 origin = cameraPos + direction; // El centro de la burbuja un metro adelante
        
        // RESTRICCIÓN: Solo buscar en opponentLayer (Eliminada la capa Default para evitar bugs)
        Collider[] hits = Physics.OverlapSphere(origin, slapRadius, opponentLayer);
        bool hitSuccessful = false;

        TeamMember myTeam = GetComponent<TeamMember>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            // PROTECCIÓN DE FUEGO AMIGO (Task 3.4)
            TeamMember targetTeam = hit.GetComponent<TeamMember>();
            bool isTeamMode = GameManager.Instance != null && GameManager.Instance.isTeamMode.Value;

            if (myTeam != null && targetTeam != null)
            {
                if (isTeamMode)
                {
                    // En modo equipos, ignorar si son del mismo ID
                    if (myTeam.teamId.Value == targetTeam.teamId.Value) continue;
                }
                else
                {
                    // En modo FFA, solo ignorar si soy yo mismo (failsafe adicional)
                    if (targetTeam.NetworkObjectId == myTeam.NetworkObjectId) continue;
                }
            }

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
