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
        UpdateCameraReference();
        _myTeam = GetComponentInParent<TeamMember>();
        
        if (nameText == null) nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (_myTeam != null)
        {
            Debug.Log($"<color=white>[NameTag] Spawned for: {_myTeam.gameObject.name}</color>");
            
            // Suscribirse a cambios futuros
            _myTeam.teamId.OnValueChanged += (oldVal, newVal) => UpdateVisuals();
            _myTeam.nickname.OnValueChanged += (oldVal, newVal) => UpdateName();
            
            // ACTUALIZACIÓN INICIAL INMEDIATA
            // Útil para cuando el objeto ya tiene datos al aparecer (ej. bots o late-joining)
            UpdateVisuals();
            UpdateName();
        }
    }

    private void UpdateCameraReference()
    {
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
        else
        {
            // Búsqueda de emergencia: Si no hay MainCamera, pillar la primera que exista
            Camera anyCam = Object.FindAnyObjectByType<Camera>();
            if (anyCam != null)
            {
                _mainCameraTransform = anyCam.transform;
                Debug.Log($"<color=orange>[NameTag] No MainCamera tag found, using '{anyCam.name}' as fallback.</color>");
            }
        }
    }

    private void LateUpdate()
    {
        if (_mainCameraTransform == null)
        {
            UpdateCameraReference();
            if (_mainCameraTransform == null) return;
        }

        // Billboarding Perfecto: Copiar la rotación de la cámara
        // Esto hace que el texto siempre sea paralelo al plano de la pantalla
        transform.rotation = _mainCameraTransform.rotation;
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
