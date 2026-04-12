using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EjectionZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if we are in a Networked session
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                TeamMember team = other.GetComponent<TeamMember>();
                if (team != null)
                {
                    GameManager.Instance.OnPlayerEliminated(team);
                }

                Debug.Log($"Network Entity {netObj.OwnerClientId} fell out of bounds!");
                netObj.Despawn(true);
            }
        }
        else
        {
            // Local Mode logic
            if (other.CompareTag("Player"))
            {
                // DESACTIVADO: Ahora el PlayerController maneja su propio Respawn (Punto 2)
                // para evitar que se reinicie la escena y se rompa el entrenamiento de la IA.
                Debug.Log($"{other.name} fell out! PlayerController will handle respawn.");
                // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            // Note: EnemyAgent handles its own reset in its internal Update() 
            // so we don't need to reload the scene for it.
        }
    }
}
