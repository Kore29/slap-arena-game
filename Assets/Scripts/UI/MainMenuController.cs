using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public UIDocument uiDocument;

    [Header("Game Mode Data Settings")]
    public GameModeData mode1vs1;
    public GameModeData modeTeams;
    public GameModeData modeFFA;

    [Header("Lobby Settings")]
    public GameObject lobbyUIParent;

    private Button _btn1vs1;
    private Button _btnTeams;
    private Button _btnFFA;
    private Button _joinBtn;
    private TextField _relayCodeInput;
    private Label _statusLabel;
    private VisualElement _container;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;
        
        _container = root.Q<VisualElement>(className: "container");
        
        _btn1vs1 = root.Q<Button>("btn-1vs1");
        _btnTeams = root.Q<Button>("btn-teams");
        _btnFFA = root.Q<Button>("btn-ffa");
        
        _joinBtn = root.Q<Button>("join-btn");
        _relayCodeInput = root.Q<TextField>("relay-code-input");
        _statusLabel = root.Q<Label>("status-label");

        _btn1vs1.clicked += On1vs1Clicked;
        _btnTeams.clicked += OnTeamsClicked;
        _btnFFA.clicked += OnFFAClicked;
        _joinBtn.clicked += OnJoinGameClicked;

        // Asegurarse de que el lobby esté oculto al inicio
        if (lobbyUIParent != null) lobbyUIParent.SetActive(false);
    }

    private void OnDisable()
    {
        if (_btn1vs1 != null) _btn1vs1.clicked -= On1vs1Clicked;
        if (_btnTeams != null) _btnTeams.clicked -= OnTeamsClicked;
        if (_btnFFA != null) _btnFFA.clicked -= OnFFAClicked;
        if (_joinBtn != null) _joinBtn.clicked -= OnJoinGameClicked;
    }

    private void On1vs1Clicked() => OnModeSelected(mode1vs1);
    private void OnTeamsClicked() => OnModeSelected(modeTeams);
    private void OnFFAClicked() => OnModeSelected(modeFFA);

    private void OnModeSelected(GameModeData mode)
    {
        if (mode == null)
        {
            _statusLabel.text = "Error: GameModeData not assigned!";
            return;
        }

        GameManager.Instance.currentModeData = mode;
        StartHostSession(mode);
    }

    private async void StartHostSession(GameModeData mode)
    {
        _statusLabel.text = $"Creating {mode.modeName} session...";
        SetButtonsEnabled(false);

        string code = await SessionManager.Instance.CreateRelaySession(mode.maxPlayers);
        
        if (!string.IsNullOrEmpty(code))
        {
            _statusLabel.text = $"Room Code: {code}";
            ShowLobbyUI(code);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.InitializeNetworkedSession(false);
            }
        }
        else
        {
            _statusLabel.text = "Failed to create session.";
            SetButtonsEnabled(true);
        }
    }

    private async void OnJoinGameClicked()
    {
        string code = _relayCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            _statusLabel.text = "Please enter a valid code.";
            return;
        }

        _statusLabel.text = "Joining session...";
        SetButtonsEnabled(false);

        bool success = await SessionManager.Instance.JoinRelaySession(code);
        
        if (success)
        {
            _statusLabel.text = "Connected!";
            ShowLobbyUI(code);
        }
        else
        {
            _statusLabel.text = "Failed to join session. Check code.";
            SetButtonsEnabled(true);
        }
    }

    private void ShowLobbyUI(string code)
    {
        _container.style.display = DisplayStyle.None;
        if (lobbyUIParent != null)
        {
            lobbyUIParent.SetActive(true);
            var lobby = lobbyUIParent.GetComponent<LobbyController>();
            if (lobby == null) lobby = lobbyUIParent.GetComponentInChildren<LobbyController>();
            if (lobby != null) lobby.SetRoomCode(code);
        }
    }

    private void SetButtonsEnabled(bool isEnabled)
    {
        _btn1vs1.SetEnabled(isEnabled);
        _btnTeams.SetEnabled(isEnabled);
        _btnFFA.SetEnabled(isEnabled);
        _joinBtn.SetEnabled(isEnabled);
        _relayCodeInput.SetEnabled(isEnabled);
    }
}
