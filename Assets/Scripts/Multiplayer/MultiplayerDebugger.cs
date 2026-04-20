using UnityEngine;
using Unity.Netcode;

public class MultiplayerDebugger : NetworkBehaviour
{
    private void OnGUI()
    {
        // Solo dibujamos si el objeto está spawned
        if (!IsSpawned) return;

        // Calculamos la posición en pantalla encima del jugador
        Vector3 worldPos = transform.position + Vector3.up * 2.5f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0) // Si el jugador está frente a la cámara
        {
            string label = IsOwner ? "OWNER (ME)" : "REMOTE PLAYER";
            string info = $"{label}\nID: {OwnerClientId}\nSpawned: {IsSpawned}";

            // Estilo del texto
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = IsOwner ? Color.green : Color.red;
            style.alignment = TextAnchor.MiddleCenter;

            // Dibujar sombra y texto
            Rect rect = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 50, 200, 100);
            GUI.color = Color.black;
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), info, style);
            GUI.color = Color.white;
            GUI.Label(rect, info, style);
        }
    }
}
