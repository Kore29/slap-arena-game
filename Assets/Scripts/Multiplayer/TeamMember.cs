using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TeamMember : NetworkBehaviour
{
    public NetworkVariable<int> teamId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> nickname = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void SetData(int id, Unity.Collections.FixedString32Bytes name)
    {
        if (IsServer)
        {
            teamId.Value = id;
            nickname.Value = name;
            Debug.Log($"<color=green>Data assigned to {gameObject.name}: {name} (Team {id})</color>");
        }
    }
}
