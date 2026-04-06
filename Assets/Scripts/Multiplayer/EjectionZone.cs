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
                Debug.Log($"Network Entity {netObj.OwnerClientId} fell out of bounds!");
                netObj.Despawn(true);
            }
        }
        else
        {
            // Local Mode logic
            if (other.CompareTag("Player"))
            {
                Debug.Log($"{other.name} fell out! Restarting practice...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            // Note: EnemyAgent handles its own reset in its internal Update() 
            // so we don't need to reload the scene for it.
        }
    }
}
