using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _cooldownFill;
    private Label _hitLog;
    private PlayerController _localPlayer;

    private Coroutine _logCoroutine;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        
        if (_uiDocument.rootVisualElement != null)
        {
            _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void Start()
    {
        // Failsafe: Si en 1 segundo no nos han inicializado, intentamos buscarnos nosotros
        InvokeRepeating(nameof(TryAutoInitialize), 1f, 1f);
    }

    private void TryAutoInitialize()
    {
        if (_localPlayer != null) 
        {
            CancelInvoke(nameof(TryAutoInitialize));
            return;
        }

        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                Initialize(p);
                Debug.Log("<color=cyan>[GameHUD] Auto-Initialized Failsafe success!</color>");
                break;
            }
        }
    }

    public void Initialize(PlayerController player)
    {
        _localPlayer = player;
        
        // Acceder a los elementos ahora que sabemos que el HUD debe mostrarse
        var root = _uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.Flex; // Mostrar HUD

        _cooldownFill = root.Q<VisualElement>("cooldown-bar-fill");
        _hitLog = root.Q<Label>("hit-log-label");

        // Suscribirse al evento de hit (Punto 2.2)
        if (_localPlayer != null)
        {
            _localPlayer.OnSlapHit += ShowHitFeedback;
        }
    }

    private void OnDisable()
    {
        if (_localPlayer != null)
        {
            _localPlayer.OnSlapHit -= ShowHitFeedback;
        }
    }

    private void Update()
    {
        if (_localPlayer == null || _cooldownFill == null) return;

        // Actualizar barra de cooldown (Punto 2.1)
        float progress = _localPlayer.CooldownProgress;
        _cooldownFill.style.width = Length.Percent(progress * 100f);

        // Cambiar color si está listo
        _cooldownFill.style.backgroundColor = progress >= 1f ? new Color(0, 1, 0.5f) : new Color(1, 0.5f, 0);
    }

    private void ShowHitFeedback()
    {
        if (_logCoroutine != null) StopCoroutine(_logCoroutine);
        _logCoroutine = StartCoroutine(HitFeedbackRoutine());
    }

    private IEnumerator HitFeedbackRoutine()
    {
        _hitLog.text = "¡ZAS!";
        _hitLog.AddToClassList("active-log");
        
        yield return new WaitForSeconds(1.0f);
        
        _hitLog.RemoveFromClassList("active-log");
        _hitLog.text = "";
    }
}
