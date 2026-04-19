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

    private void OnEnable()
    {
        if (resultsUIDoc == null) resultsUIDoc = GetComponent<UIDocument>();
        var root = resultsUIDoc.rootVisualElement;

        _overlay = root.Q<VisualElement>("results-overlay");
        _titleLabel = root.Q<Label>("result-title");
        _winnerLabel = root.Q<Label>("winner-name");
        _backBtn = root.Q<Button>("back-to-lobby-btn");

        _backBtn.clicked += OnBackClicked;
        _overlay.style.display = DisplayStyle.None;
    }

    public void ShowResults(string winnerName, int winnerTeam, ulong winnerClientId)
    {
        _overlay.style.display = DisplayStyle.Flex;
        _winnerLabel.text = winnerName;

        // Sincronizar victoria/derrota
        TeamMember localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject?.GetComponent<TeamMember>();
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
                // En FFA, la victoria es por ClientId individual
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
        else if (winnerTeam == -1 && winnerClientId == 999)
        {
            _titleLabel.text = "DRAW";
            _titleLabel.style.color = Color.white;
            _titleLabel.style.unityTextOutlineWidth = 0;
        }
        else
        {
            _titleLabel.text = "DEFEAT";
            _titleLabel.style.color = new StyleColor(new Color(1f, 0.2f, 0.2f)); // Red
            _titleLabel.style.unityTextOutlineColor = new StyleColor(new Color(0.5f, 0f, 0f, 0.5f));
            _titleLabel.style.unityTextOutlineWidth = 2f;
        }

        // Solo el Host puede ver el botón de volver (o todos, pero el host controla el cambio de escena)
        _backBtn.SetEnabled(NetworkManager.Singleton.IsServer);
        if (!NetworkManager.Singleton.IsServer) _backBtn.text = "WAITING FOR HOST...";
    }

    private void OnBackClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Volver al menú principal sincronizadamente
            NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
