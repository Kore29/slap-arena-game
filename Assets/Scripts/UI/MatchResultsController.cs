using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class MatchResultsController : MonoBehaviour
{
    [SerializeField] private UIDocument resultsUIDoc;
    
    private VisualElement _overlay;
    private Label _titleLabel;
    private Label _winnerLabel;
    private Button _backBtn;

    private bool _isInitialized = false;

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (_isInitialized) return;

        if (resultsUIDoc == null) resultsUIDoc = GetComponent<UIDocument>();
        if (resultsUIDoc == null) return;

        var root = resultsUIDoc.rootVisualElement;
        if (root == null) return;

        _overlay = root.Q<VisualElement>("results-overlay");
        _titleLabel = root.Q<Label>("result-title");
        _winnerLabel = root.Q<Label>("winner-name");
        _backBtn = root.Q<Button>("back-to-lobby-btn");

        if (_backBtn != null)
        {
            _backBtn.clicked -= OnBackClicked; // Seguridad contra dobles registros
            _backBtn.clicked += OnBackClicked;
        }

        if (_overlay != null)
        {
            _overlay.style.display = DisplayStyle.None;
            _isInitialized = true;
        }
    }

    public void ShowResults(string winnerName, int winnerTeam, ulong winnerClientId)
    {
        InitializeUI();
        if (!_isInitialized) return;

        _overlay.style.display = DisplayStyle.None; // Reset visual
        _overlay.style.display = DisplayStyle.Flex;
        if (_winnerLabel != null) _winnerLabel.text = winnerName;

        UpdateStatusText(winnerTeam, winnerClientId);
        HideGameplayHUD();

        if (_backBtn != null)
        {
            _backBtn.SetEnabled(true);
            _backBtn.text = "EXIT TO MENU";
        }
    }

    public void ShowEliminatedEarly()
    {
        InitializeUI();
        if (!_isInitialized || _overlay == null) return;
        if (_overlay.style.display == DisplayStyle.Flex) return;

        _overlay.style.display = DisplayStyle.Flex;
        
        if (_titleLabel != null)
        {
            _titleLabel.text = "DEFEAT";
            _titleLabel.style.color = new StyleColor(new Color(1f, 0.2f, 0.2f));
        }
        
        if (_winnerLabel != null) _winnerLabel.text = "YOU FELL OUT";
        HideGameplayHUD();
        
        if (_backBtn != null)
        {
            _backBtn.SetEnabled(true);
            _backBtn.text = "EXIT TO MENU";
        }
    }

    private void UpdateStatusText(int winnerTeam, ulong winnerClientId)
    {
        if (!_isInitialized || _titleLabel == null) return;

        TeamMember localPlayer = null;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject?.GetComponent<TeamMember>();
        }

        bool isTeamMode = GameManager.Instance != null && GameManager.Instance.isTeamMode.Value;
        bool didIWin = false;

        if (localPlayer != null)
        {
            if (isTeamMode)
            {
                didIWin = (localPlayer.teamId.Value == winnerTeam);
            }
            else
            {
                didIWin = (NetworkManager.Singleton.LocalClientId == winnerClientId);
            }
        }

        if (didIWin)
        {
            _titleLabel.text = "VICTORY";
            _titleLabel.style.color = new StyleColor(new Color(1f, 0.84f, 0f)); // Gold
            _titleLabel.style.unityTextOutlineColor = new StyleColor(new Color(1f, 0.5f, 0f, 0.5f));
            _titleLabel.style.unityTextOutlineWidth = 2f;
        }
        else
        {
            bool isDraw = (winnerTeam == -1 && winnerClientId == 999);
            _titleLabel.text = isDraw ? "DRAW" : "DEFEAT";
            _titleLabel.style.color = isDraw ? Color.white : new StyleColor(new Color(1f, 0.2f, 0.2f));
            _titleLabel.style.unityTextOutlineWidth = isDraw ? 0 : 2f;
            if (!isDraw) {
                _titleLabel.style.unityTextOutlineColor = new StyleColor(new Color(0.5f, 0f, 0f, 0.5f));
            }
        }
    }

    private void HideGameplayHUD()
    {
        GameHUD hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.SetVisibility(false);
    }

    private void OnBackClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CleanupAndReturnToMenu();
        }
        else
        {
            // Fallback extremo
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
