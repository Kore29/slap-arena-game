using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;
    private Quaternion _targetRotation;
    
    // --- NUEVAS VARIABLES DE SINCRONIZACIÓN ---
    private Vector3 _lastPosition;
    private float _currentSpeed;

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
        _lastPosition = transform.position;
        
        if (IsOwner)
        {
            Debug.Log($"Network Player Spawned (OWNER): {OwnerClientId}");
            SetupLocalPlayer();
        }
        else
        {
            // --- PROTECCIÓN MULTIJUGADOR (Fix Espejo/Cámara) ---
            // Desactivamos la cámara interna y el AudioListener de los demás jugadores
            if (playerCamera != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;
                
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
                
                Debug.Log($"Disabled remote player camera/audio for ID: {OwnerClientId}");
            }

            // --- PROTECCIÓN FÍSICA (Fix Caída Infinita) ---
            // Si no somos dueños, el Rigidbody debe ser KINEMATIC para no pelear con la red
            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
                Debug.Log($"Physics disabled for remote player ID: {OwnerClientId}");
            }
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
        // Aseguramos que el dueño NO es kinematic para que caiga al suelo (Fix Vuelo Spawn)
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

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
        // PRIORIDAD: Si no somos dueños en Red, ignorar input por completo (Fix Espejo)
        if (IsNetworkActive && !IsOwner) return;

        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        // SEGURIDAD: Solo permitir control si estamos en estado Playing (Fix Congelación)
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            // Opcional: Desbloquear cursor si detectamos que no estamos jugando pero somos dueños
            if (UnityEngine.Cursor.lockState != CursorLockMode.None)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
            return;
        }

        HandleLook();
        HandleSlapInput();

        // Se ha movido a FixedUpdate para evitar conflictos de teletransporte (Punto 2.2)
    }
    private void FixedUpdate()
    {
        // --- CÁLCULO DE VELOCIDAD PARA ANIMACIONES (Multiplayer Fix) ---
        // Calculamos cuánto se ha movido el personaje independientemente de si es owner o no
        Vector3 deltaPosition = transform.position - _lastPosition;
        deltaPosition.y = 0; // Solo nos importa el movimiento horizontal
        float speed = deltaPosition.magnitude / Time.fixedDeltaTime;
        _currentSpeed = Mathf.Lerp(_currentSpeed, speed, Time.fixedDeltaTime * 10f); // Suavizado
        _lastPosition = transform.position;

        // Actualizar animador para TODOS los clientes (Task 3.1)
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            // Deteción de suelo más generosa
            bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.2f);
            
            _animator.SetFloat("Speed", _currentSpeed / movementSpeed);
            _animator.SetBool("IsFalling", transform.position.y < -1.0f); // Solo falling si estamos en el abismo
            _animator.SetBool("Grounded", isGrounded); 
        }
        else {
            Debug.LogWarning("ANIM ERROR: No se encuentra el componente Animator en el Player!");
        }

        // PRIORIDAD: Si no somos dueños en Red, ignorar físicas por completo (Fix Espejo)
        if (IsNetworkActive && !IsOwner) return;

        bool hasControl = !IsNetworkActive || IsOwner;
        if (!hasControl) return;

        // SEGURIDAD: Bloqueo físico si no estamos jugando
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            _rb.linearVelocity = Vector3.zero; // Evitar que se deslicen
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true; // Congelar físicas para no caer al abismo durante resultados
            return;
        }
        
        // Restaurar física si volvemos a jugar (por si acaso se reutiliza el objeto)
        if (_rb.isKinematic) _rb.isKinematic = false;

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
            
            // --- FEEDBACK LOCAL INMEDIATO ---
            if (_animator != null) _animator.SetTrigger("Slap");
            
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
        
        // --- SINCRONIZACIÓN DE ANIMACIÓN (Task 3.1) ---
        // Avisamos a todos los clientes que este jugador está golpeando
        PlaySlapAnimationClientRpc();
        
        Debug.Log("Executing Slap logic on Server...");
        
        Vector3 cameraPos = playerCamera != null ? playerCamera.position : transform.position + Vector3.up * 0.8f;
        Vector3 origin = cameraPos + direction; // El centro de la burbuja un metro adelante
        
        // RESTRICCIÓN: Solo buscar en opponentLayer (Eliminada la capa Default para evitar bugs)
        Collider[] hits = Physics.OverlapSphere(origin, slapRadius, opponentLayer);
        bool hitSuccessful = false;

        TeamMember myTeam = GetComponent<TeamMember>();

        foreach (var hit in hits)
        {
            // SEGURIDAD MULTIJUGADOR: Ignorar si por casualidad nos golpeamos a nosotros mismos
            if (hit.gameObject == gameObject) continue;
            
            // --- MEJORA DE DETECCIÓN (Task 3.3) ---
            // Buscamos el componente en el objeto golpeado o en sus padres (por si el collider está en un hijo)
            TeamMember targetTeam = hit.GetComponent<TeamMember>();
            if (targetTeam == null) targetTeam = hit.GetComponentInParent<TeamMember>();
            
            bool isTeamMode = GameManager.Instance != null && GameManager.Instance.isTeamMode.Value;

            if (targetTeam != null)
            {
                if (isTeamMode)
                {
                    if (myTeam != null && myTeam.teamId.Value == targetTeam.teamId.Value) 
                    {
                        Debug.Log($"Ignored {hit.name} because they are team {targetTeam.teamId.Value}");
                        continue;
                    }
                }
                else
                {
                    if (myTeam != null && targetTeam.NetworkObjectId == myTeam.NetworkObjectId) continue;
                }

                // --- LÓGICA DE IMPACTO MIXTA (Task 3.3) ---
                PlayerController targetController = hit.GetComponent<PlayerController>();
                if (targetController == null) targetController = hit.GetComponentInParent<PlayerController>();
                
                if (targetController != null)
                {
                    // CASO A: Es un jugador humano (usar mensaje de red)
                    Vector3 knockbackForce = direction * slapForce;
                    targetController.ApplyKnockbackClientRpc(knockbackForce);
                    Debug.Log($"<color=cyan>Slap RPC sent to Human: {targetController.name}</color>");
                    hitSuccessful = true;
                }
                else
                {
                    // CASO B: Es una IA o objeto físico (usar fuerza directa en el servidor)
                    Rigidbody targetRb = hit.GetComponent<Rigidbody>();
                    if (targetRb == null) targetRb = hit.GetComponentInParent<Rigidbody>();
                    
                    if (targetRb != null)
                    {
                        targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
                        Debug.Log($"<color=orange>Slap Physical Force applied to AI/Obj: {hit.name}</color>");
                        hitSuccessful = true;
                    }
                }
            }
            else
            {
                // CASO C: Objeto sin TeamMember (ej: cajas, barriles)
                Rigidbody targetRb = hit.GetComponent<Rigidbody>();
                if (targetRb == null) targetRb = hit.GetComponentInParent<Rigidbody>();
                
                if (targetRb != null)
                {
                    targetRb.AddForce(direction * slapForce, ForceMode.Impulse);
                    hitSuccessful = true;
                }
            }
        }

        if (hitSuccessful)
        {
            // Avisar al cliente atacante para que muestre el "ZAS" (Task 3.5)
            NotifySlapHitClientRpc();
        }
    }

    [ClientRpc]
    public void ApplyKnockbackClientRpc(Vector3 force)
    {
        // Solo el dueño local del personaje procesa el empujón
        if (!IsOwner) return;

        if (_rb != null)
        {
            // Nos aseguramos de que no somos kinematic momentáneamente para recibir el impacto
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic = false;
            
            // Limpiamos velocidad previa para que el impacto se sienta sólido
            _rb.linearVelocity = Vector3.zero;
            
            _rb.AddForce(force, ForceMode.Impulse);
            Debug.Log($"<color=red>¡RECIBISTE UN GOLPE! Fuerza: {force.magnitude}</color>");
            
            // No volvemos a kinematic inmediatamente, dejamos que la física actúe 
            // El propio Update de movimiento lo volverá a poner a false si es necesario
        }
    }

    private System.Collections.IEnumerator ApplySlapKick()
    {
        if (playerCamera == null) yield break;
        
        Vector3 originalPos = playerCamera.localPosition;
        playerCamera.localPosition -= Vector3.forward * 0.15f; 
        yield return new WaitForSeconds(0.05f);
        playerCamera.localPosition = originalPos;
    }

    [ClientRpc]
    private void PlaySlapAnimationClientRpc()
    {
        // El dueño ya la ha ejecutado localmente para no tener lag
        if (IsOwner && IsClient) return;

        if (_animator != null)
        {
            _animator.SetTrigger("Slap");
            Debug.Log($"<color=magenta>!!! ANIMATION RPC received for {name}</color>");
        }
    }

    [ClientRpc]
    private void NotifySlapHitClientRpc()
    {
        // Solo el dueño (el que dio el golpe) muestra el mensaje de ZAS y el kick de cámara
        if (IsOwner)
        {
            OnSlapHit?.Invoke();
            StartCoroutine(ApplySlapKick());
            Debug.Log("<color=yellow>¡ZAS! Feedback triggered on Attacker's Screen</color>");
        }
    }
}
