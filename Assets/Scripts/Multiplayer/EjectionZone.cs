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
            if (other.CompareTag("Player") || other.GetComponent<EnemyAgent>() != null)
            {
                Debug.Log($"{other.name} fell out! Restarting practice...");
                // In local practice, we can just reload the scene to reset everything
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
