using UnityEngine;

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
    }
}
