using UnityEngine;
using TMPro;
using Unity.Netcode;

public class WorldNameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Color allyColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;

    private Transform _mainCameraTransform;
    private TeamMember _myTeam;

    public override void OnNetworkSpawn()
    {
        _mainCameraTransform = Camera.main != null ? Camera.main.transform : null;
        _myTeam = GetComponentInParent<TeamMember>();
        
        if (nameText == null) nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (_myTeam != null)
        {
            Debug.Log($"<color=white>Spawned NameTag for: {_myTeam.gameObject.name}</color>");
            
            // Suscribirse a los cambios en la red
            _myTeam.teamId.OnValueChanged += (oldVal, newVal) => UpdateVisuals();
            _myTeam.nickname.OnValueChanged += (oldVal, newVal) => UpdateName();
            
            // Inicializar (con un ligero retraso para asegurar que la red esté lista)
            Invoke(nameof(ForceInitialUpdate), 0.2f);
        }
        else
        {
            Debug.LogWarning($"NameTag on {gameObject.name} could not find TeamMember in parent!");
        }
    }

    private void ForceInitialUpdate()
    {
        UpdateVisuals();
        UpdateName();
    }

    private void LateUpdate()
    {
        if (_mainCameraTransform != null)
        {
            transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                             _mainCameraTransform.rotation * Vector3.up);
        }
    }

    public void UpdateVisuals()
    {
        if (_myTeam == null || nameText == null) return;
        
        // Lógica simple por ahora: Equipo 0 es azul, Equipo 1 es rojo
        nameText.color = (_myTeam.teamId.Value == 0) ? allyColor : enemyColor;
    }

    private void UpdateName()
    {
        if (nameText != null && _myTeam != null) 
            nameText.text = _myTeam.nickname.Value.ToString();
    }
}
