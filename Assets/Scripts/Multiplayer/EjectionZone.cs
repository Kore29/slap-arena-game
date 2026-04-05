using Unity.Netcode;
using UnityEngine;

public class EjectionZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Require server authority to handle logic
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (other.CompareTag("Player") || other.GetComponent<NetworkObject>() != null)
        {
            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                Debug.Log($"Player {netObj.OwnerClientId} fell out of bounds!");
                // Despawn or resetplayer. For a brawler, falling out means despawn/eliminated.
                netObj.Despawn(true);
            }
        }
    }
}
