using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System;
using Unity.Collections;

[Serializable]
public struct LobbySlot : INetworkSerializable, IEquatable<LobbySlot>
{
    public ulong ClientId;
    public FixedString32Bytes Nickname;
    public int TeamId; // 0 para A, 1 para B (o -1 para FFA)
    public bool IsBot;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref TeamId);
        serializer.SerializeValue(ref IsBot);
        serializer.SerializeValue(ref IsReady);
    }

    public bool Equals(LobbySlot other)
    {
        return ClientId == other.ClientId && IsBot == other.IsBot;
    }
}

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }
    
    public NetworkList<LobbySlot> lobbySlots;

    private void Awake()
    {
        lobbySlots = new NetworkList<LobbySlot>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Independizar del padre [MANAGERS] para evitar errores de Unity
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected to lobby: {clientId}");
        if (IsServer)
        {
            AssignSlotToClient(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected from lobby: {clientId}");
        if (IsServer)
        {
            RemoveClientFromSlot(clientId);
        }
    }

    private void AssignSlotToClient(ulong clientId)
    {
        // 1. Buscar si ya tiene un slot (por si acaso)
        for (int i = 0; i < lobbySlots.Count; i++)
        {
            if (lobbySlots[i].ClientId == clientId && !lobbySlots[i].IsBot) return;
        }

        // 2. Buscar un slot de un BOT para reemplazarlo o un slot vacío
        for (int i = 0; i < lobbySlots.Count; i++)
        {
            if (lobbySlots[i].IsBot)
            {
                var slot = lobbySlots[i];
                slot.ClientId = clientId;
                slot.IsBot = false;
                slot.Nickname = $"Player {clientId}"; // TODO: Obtener nickname real
                lobbySlots[i] = slot;
                Debug.Log($"Replaced bot in slot {i} with client {clientId}");
                return;
            }
        }

        // 3. Si no hay bots que reemplazar, añadir al final si hay espacio
        if (lobbySlots.Count < 8) // Max global slots
        {
            lobbySlots.Add(new LobbySlot
            {
                ClientId = clientId,
                Nickname = $"Player {clientId}",
                IsBot = false,
                TeamId = lobbySlots.Count % 2, // Alternar equipos
                IsReady = false
            });
        }
    }

    private void RemoveClientFromSlot(ulong clientId)
    {
        for (int i = 0; i < lobbySlots.Count; i++)
        {
            if (lobbySlots[i].ClientId == clientId && !lobbySlots[i].IsBot)
            {
                // Si la lógica de bots está activa, lo reemplazamos por un bot
                if (GameManager.Instance.currentModeData.fillWithBots)
                {
                    var slot = lobbySlots[i];
                    slot.IsBot = true;
                    slot.ClientId = 999 + (ulong)i;
                    slot.Nickname = BotNameUtility.GetRandomName();
                    lobbySlots[i] = slot;
                }
                else
                {
                    lobbySlots.RemoveAt(i);
                }
                break;
            }
        }
    }

    public void InitializeLobby(GameModeData mode)
    {
        if (!IsServer) return;

        lobbySlots.Clear();

        // Añadir al Host (nosotros)
        lobbySlots.Add(new LobbySlot
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            Nickname = "Host",
            TeamId = 0,
            IsBot = false,
            IsReady = true
        });

        // Rellenar con Bots si es necesario
        if (mode.fillWithBots)
        {
            int botsToSpawn = mode.maxPlayers - 1;
            for (int i = 0; i < botsToSpawn; i++)
            {
                lobbySlots.Add(new LobbySlot
                {
                    ClientId = 999 + (ulong)i,
                    Nickname = BotNameUtility.GetRandomName(),
                    TeamId = (i + 1) % 2, // Alternar equipos (el host es 0)
                    IsBot = true,
                    IsReady = true
                });
            }
        }
    }

    private async void Start()
    {
        await AuthenticatePlayer();
    }

    private async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("Error authenticating: " + e.Message);
        }
    }

    public async Task<string> CreateRelaySession(int maxPlayers = 2)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4, 
                (ushort)allocation.RelayServer.Port, 
                allocation.AllocationIdBytes, 
                allocation.Key, 
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            Debug.Log("Relay Session Created with Join Code: " + joinCode);
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay create exception: " + e.Message);
            return null;
        }
    }

    public async Task<bool> JoinRelaySession(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4, 
                (ushort)joinAllocation.RelayServer.Port, 
                joinAllocation.AllocationIdBytes, 
                joinAllocation.Key, 
                joinAllocation.ConnectionData, 
                joinAllocation.HostConnectionData);

            bool joined = NetworkManager.Singleton.StartClient();
            return joined;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay join exception: " + e.Message);
            return false;
        }
    }
}
