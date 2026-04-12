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
    private ScrollView _teamAList;
    private ScrollView _teamBList;
    private ScrollView _ffaList;
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

    private void InitializeUI()
    {
        if (lobbyUIDoc == null) lobbyUIDoc = GetComponent<UIDocument>();
        var root = lobbyUIDoc.rootVisualElement;

        _teamsParent = root.Q<VisualElement>("teams-container");
        _ffaParent = root.Q<VisualElement>("ffa-container");
        _teamAList = root.Q<ScrollView>("team-a-list");
        _teamBList = root.Q<ScrollView>("team-b-list");
        _ffaList = root.Q<ScrollView>("ffa-list");
        _roomCodeLabel = root.Q<Label>("room-code-label");
        _startBtn = root.Q<Button>("start-btn");
        _leaveBtn = root.Q<Button>("leave-btn");
        _switchGlobalBtn = root.Q<Button>("switch-global-btn");
        _removeBotsBtn = root.Q<Button>("remove-bots-btn");

        _leaveBtn.clicked += OnLeaveClicked;
        _startBtn.clicked += OnStartClicked;
        _switchGlobalBtn.clicked += OnSwitchGlobalClicked;
        _removeBotsBtn.clicked += OnRemoveBotsClicked;

        // Solo el Host puede usar botones de gestión
        bool isHost = NetworkManager.Singleton.IsServer;
        _startBtn.SetEnabled(isHost);
        _removeBotsBtn.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
        
        if (!isHost) _startBtn.style.display = DisplayStyle.None;

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
