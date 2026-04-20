using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class GameBootstrap : MonoBehaviour
{
    [Header("Managers Prefab (El que tiene todos los scripts)")]
    public GameObject managersPrefab;

    private void Awake()
    {
        // Si ya hay un Manager en el juego (porque venimos de otra escena), no hacemos nada.
        if (GameManager.Instance != null) 
        {
            Debug.Log("<color=cyan>ℹ GameBootstrap: Managers ya detectados, omitiendo.</color>");
            return;
        }

        // Si no hay Manager, lo creamos desde el Prefab.
        if (managersPrefab != null)
        {
            Debug.Log("<color=green>🚀 GameBootstrap: Instanciando Managers...</color>");
            GameObject go = Instantiate(managersPrefab);
            go.name = "[MANAGERS]";
        }
        else
        {
            Debug.LogError("❌ GameBootstrap: ¡Falta asignar el Prefab en el Inspector!");
        }

        // --- AUTO-EASY: Asegurar que hay un EventSystem (Tarea 1.4) ---
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem (Auto-Generated)");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("<color=orange>⚠ EventSystem faltante: Creado automáticamente con InputSystemUIInputModule.</color>");
            DontDestroyOnLoad(es);
        }
    }
}
