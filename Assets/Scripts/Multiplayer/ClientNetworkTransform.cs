using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// Proporciona autoridad al cliente sobre su propio transform. 
/// Esencial para juegos con físicas (Rigidbody) donde el jugador debe moverse localmente.
/// </summary>
[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    /// <summary>
    /// Indica a Netcode que este objeto NO es autoritario para el servidor
    /// cuando el cliente que posee el objeto está enviando actualizaciones.
    /// </summary>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
