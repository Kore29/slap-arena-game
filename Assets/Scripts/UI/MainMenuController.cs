using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public UIDocument uiDocument;

    private Button _practiceBtn;
    private Button _coopBtn;
    private Button _createBtn;
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
        _practiceBtn = root.Q<Button>("practice-btn");
        _coopBtn = root.Q<Button>("coop-btn");
        _createBtn = root.Q<Button>("create-btn");
        _joinBtn = root.Q<Button>("join-btn");
        _relayCodeInput = root.Q<TextField>("relay-code-input");
        _statusLabel = root.Q<Label>("status-label");

        _practiceBtn.clicked += OnPracticeClicked;
        _coopBtn.clicked += OnCoopClicked;
        _createBtn.clicked += OnCreateSessionClicked;
        _joinBtn.clicked += OnJoinGameClicked;
    }

    private void OnDisable()
    {
        _practiceBtn.clicked -= OnPracticeClicked;
        _coopBtn.clicked -= OnCoopClicked;
        _createBtn.clicked -= OnCreateSessionClicked;
        _joinBtn.clicked -= OnJoinGameClicked;
    }

    private void OnPracticeClicked()
    {
        _container.style.display = DisplayStyle.None;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLocalGame();
        }
        
        // APAGAR EL MENÚ: Evita que el menú siga escuchando clicks mientras juegas
        gameObject.SetActive(false);
    }

    private void OnCoopClicked()
    {
        OnCreateSessionClicked(true); // Host with Coop flag
    }

    private void OnCreateSessionClicked() { OnCreateSessionClicked(false); }

    private async void OnCreateSessionClicked(bool isCoop)
    {
        _statusLabel.text = isCoop ? "Creating Coop Session..." : "Creating session...";
        SetButtonsEnabled(false);

        string code = await SessionManager.Instance.CreateRelaySession(2);
        
        if (!string.IsNullOrEmpty(code))
        {
            _container.style.display = DisplayStyle.None;
            _statusLabel.text = $"Room Code: {code}";

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNetworkedGame(isCoop);
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
            _container.style.display = DisplayStyle.None;
        }
        else
        {
            _statusLabel.text = "Failed to join session. Check code.";
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool isEnabled)
    {
        _practiceBtn.SetEnabled(isEnabled);
        _coopBtn.SetEnabled(isEnabled);
        _createBtn.SetEnabled(isEnabled);
        _joinBtn.SetEnabled(isEnabled);
        _relayCodeInput.SetEnabled(isEnabled);
    }
}
