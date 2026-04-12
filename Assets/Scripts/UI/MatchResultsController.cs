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

    public void ShowResults(string winnerName, int winnerTeam)
    {
        _overlay.style.display = DisplayStyle.Flex;
        _winnerLabel.text = winnerName;

        // Comprobar si el equipo local ganó (asumiendo que podemos obtener el equipo del player local)
        TeamMember localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject?.GetComponent<TeamMember>();
        if (localPlayer != null)
        {
            if (localPlayer.teamId.Value == winnerTeam)
            {
                _titleLabel.text = "VICTORY";
                _titleLabel.style.color = new StyleColor(new Color(1f, 0.8f, 0f)); // Gold
            }
            else
            {
                _titleLabel.text = "DEFEAT";
                _titleLabel.style.color = new StyleColor(new Color(1f, 0.3f, 0.3f)); // Red
            }
        }
        else if (winnerTeam == -1)
        {
            _titleLabel.text = "DRAW";
            _titleLabel.style.color = Color.white;
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
