using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using System.Collections.Generic;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private UIDocument lobbyUIDoc;
    [SerializeField] private VisualTreeAsset lobbyItemTemplate;

    private VisualElement _teamsParent;
    private VisualElement _ffaParent;
    private VisualElement _teamAList;
    private VisualElement _teamBList;
    private VisualElement _ffaList;
    private Label _teamALabel;
    private Label _teamBLabel;
    private Label _roomCodeLabel;
    private Button _startBtn;
    private Button _leaveBtn;
    private Button _switchGlobalBtn;
    private Button _removeBotsBtn;

    public void SetRoomCode(string code)
    {
        if (_roomCodeLabel == null) InitializeUI();
        if (_roomCodeLabel != null) _roomCodeLabel.text = $"JOIN CODE: {code}";
        Debug.Log($"[LobbyUI] Room code set to: {code}");
    }
    private void OnEnable()
    {
        InitializeUI();
        
        // Desbloquear cursor para interactuar con la UI
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    public void SetVisibility(bool visible)
    {
        if (lobbyUIDoc == null) lobbyUIDoc = GetComponent<UIDocument>();
        if (lobbyUIDoc != null && lobbyUIDoc.rootVisualElement != null)
        {
            lobbyUIDoc.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Si lo ocultamos, asegurémonos de que el cursor se bloquee si ya estamos jugando
            if (!visible && GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }
        }
    }

    private void OnDisable()
    {
        if (_leaveBtn != null) _leaveBtn.clicked -= OnLeaveClicked;
        if (_startBtn != null) _startBtn.clicked -= OnStartClicked;
        if (_switchGlobalBtn != null) _switchGlobalBtn.clicked -= OnSwitchGlobalClicked;
        if (_removeBotsBtn != null) _removeBotsBtn.clicked -= OnRemoveBotsClicked;

        // Limpieza de suscripción de Red (Evita errores de referencia nula al cerrar)
        if (SessionManager.Instance != null && SessionManager.Instance.lobbySlots != null)
        {
            // Nota: OnListChanged se limpia automáticamente al destruir el objeto en Netcode,
            // pero es buena práctica si el objeto se desactiva/reactiva.
        }
    }

    private void InitializeUI()
    {
        Debug.Log("<color=cyan>[LobbyUI] Initializing UI Full-Force Mode...</color>");
        if (lobbyUIDoc == null) lobbyUIDoc = GetComponent<UIDocument>();
        var root = lobbyUIDoc.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("<color=red>[LobbyUI] rootVisualElement is NULL!</color>");
            return;
        }

        // --- FUERZA BRUTA PARA PANTALLA COMPLETA ---
        // Forzamos al ROOT real a crecer y ocupar todo
        root.style.flexGrow = 1;

        VisualElement screen = root.Q<VisualElement>("lobby-screen");
        if (screen != null)
        {
            Debug.Log("<color=green>[LobbyUI] lobby-screen linked and stretching...</color>");
            
            // Forzamos al elemento del UXML a ser absoluto y estirarse al 100%
            screen.style.position = Position.Absolute;
            screen.style.top = 0;
            screen.style.left = 0;
            screen.style.right = 0;
            screen.style.bottom = 0;
            screen.style.flexGrow = 1;
            screen.style.width = new Length(100, LengthUnit.Percent);
            screen.style.height = new Length(100, LengthUnit.Percent);

            // Diagnóstico de dimensiones en tiempo real
            screen.RegisterCallback<GeometryChangedEvent>(evt => {
                Debug.Log($"[LobbyUI] Geometry changed: {evt.newRect.width}x{evt.newRect.height}");
            });
        }
        else
        {
            Debug.LogWarning("[LobbyUI] lobby-screen NOT FOUND in UXML!");
        }


        _teamsParent = root.Q<VisualElement>("teams-container");
        _ffaParent = root.Q<VisualElement>("ffa-container");
        _teamAList = root.Q<VisualElement>("team-a-list");
        _teamBList = root.Q<VisualElement>("team-b-list");
        _ffaList = root.Q<VisualElement>("ffa-list");
        _teamALabel = root.Q<Label>("team-a-label");
        _teamBLabel = root.Q<Label>("team-b-label");
        _roomCodeLabel = root.Q<Label>("room-code-label");
        _startBtn = root.Q<Button>("start-btn");
        _leaveBtn = root.Q<Button>("leave-btn");
        _switchGlobalBtn = root.Q<Button>("switch-global-btn");
        _removeBotsBtn = root.Q<Button>("remove-bots-btn");

        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        
        if (_leaveBtn != null) _leaveBtn.clicked += OnLeaveClicked;
        if (_startBtn != null) _startBtn.clicked += OnStartClicked;
        if (_switchGlobalBtn != null) _switchGlobalBtn.clicked += OnSwitchGlobalClicked;
        if (_removeBotsBtn != null) _removeBotsBtn.clicked += OnRemoveBotsClicked;

        // Solo el Host puede usar botones de gestión
        if (_startBtn != null) _startBtn.SetEnabled(isHost);
        if (_removeBotsBtn != null) _removeBotsBtn.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
        
        if (!isHost && _startBtn != null) _startBtn.style.display = DisplayStyle.None;

        // Suscribirse a cambios en la lista de red
        if (SessionManager.Instance != null && SessionManager.Instance.lobbySlots != null)
        {
            SessionManager.Instance.lobbySlots.OnListChanged += (changeEvent) => RefreshUI();
            Debug.Log($"[LobbyUI] Subscribed to LobbySlots. Current count: {SessionManager.Instance.lobbySlots.Count}");
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentModeData == null)
        {
             Debug.LogWarning("[LobbyUI] RefreshUI called but GameManager or ModeData is NULL.");
             return;
        }

        _teamAList.Clear();
        _teamBList.Clear();
        _ffaList.Clear();

        bool isTeamMode = GameManager.Instance.currentModeData.isTeamBased;
        
        // Mostrar/Ocultar contenedores principales
        _teamsParent.style.display = isTeamMode ? DisplayStyle.Flex : DisplayStyle.None;
        _ffaParent.style.display = isTeamMode ? DisplayStyle.None : DisplayStyle.Flex;

        // Configurar etiquetas de columnas
        if (isTeamMode)
        {
            if (GameManager.Instance.currentModeData.maxPlayers == 2)
            {
                _teamALabel.text = "PLAYER 1";
                _teamBLabel.text = "PLAYER 2";
            }
            else
            {
                _teamALabel.text = "TEAM ALPHA";
                _teamBLabel.text = "TEAM BETA";
            }
        }

        Debug.Log($"[LobbyUI] Refreshing UI. Mode: {(isTeamMode ? "TEAM" : "FFA")} | Slots Found: {SessionManager.Instance.lobbySlots.Count}");

        if (SessionManager.Instance.lobbySlots.Count == 0)
        {
            Debug.LogWarning("[LobbyUI] lobbySlots is EMPTY! Is NetworkManager running?");
        }

        foreach (var slot in SessionManager.Instance.lobbySlots)
        {
            if (lobbyItemTemplate == null) { Debug.LogError("[LobbyUI] Template is MISSING!"); break; }
            
            VisualElement item = lobbyItemTemplate.Instantiate();
            string pName = string.IsNullOrEmpty(slot.Nickname.ToString()) ? $"Player {slot.ClientId}" : slot.Nickname.ToString();
            item.Q<Label>("player-name").text = pName;
            
            Debug.Log($"[LobbyUI] Adding player: {pName} to Team {slot.TeamId}");
            
            // Indicador de estado (Ready/IsBot)
            var indicator = item.Q<VisualElement>("status-indicator");
            indicator.style.backgroundColor = slot.IsReady ? new StyleColor(new Color(0.2f, 1f, 0.4f)) : new StyleColor(Color.grey);
            
            if (slot.IsBot) item.Q<VisualElement>("bot-tag").style.display = DisplayStyle.Flex;

            // Botón de cambio de equipo (solo para mí si no soy bot)
            var switchBtn = item.Q<Button>("switch-team-btn");
            if (slot.ClientId == NetworkManager.Singleton.LocalClientId && !slot.IsBot && isTeamMode)
            {
                switchBtn.style.display = DisplayStyle.Flex;
                switchBtn.clicked += () => SessionManager.Instance.ChangeTeamServerRpc(slot.ClientId, (slot.TeamId + 1) % 2);
            }

            // Añadir al contenedor correspondiente
            if (isTeamMode)
            {
                if (slot.TeamId == 0) _teamAList.Add(item);
                else _teamBList.Add(item);
            }
            else
            {
                _ffaList.Add(item);
            }
        }
    }

    private void OnStartClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameManager.Instance.StartMatch();
        }
    }

    private void OnSwitchGlobalClicked()
    {
        ulong myId = NetworkManager.Singleton.LocalClientId;
        foreach (var slot in SessionManager.Instance.lobbySlots)
        {
            if (slot.ClientId == myId && !slot.IsBot)
            {
                SessionManager.Instance.ChangeTeamServerRpc(myId, (slot.TeamId + 1) % 2);
                break;
            }
        }
    }

    private void OnRemoveBotsClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SessionManager.Instance.RemoveBotsServerRpc();
        }
    }

    private void OnLeaveClicked()
    {
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // O recargar actual
    }
}
