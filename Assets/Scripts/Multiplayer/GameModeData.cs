using UnityEngine;

[CreateAssetMenu(fileName = "NewGameMode", menuName = "SlapArena/GameModeData")]
public class GameModeData : ScriptableObject
{
    public string modeName;
    public int maxPlayers = 2;
    public bool isTeamBased = false;
    public int playersPerTeam = 1;
    
    public enum VictoryCondition { LastManStanding, LastTeamStanding, Points }
    public VictoryCondition victoryCondition = VictoryCondition.LastManStanding;

    [Header("Bot Settings")]
    public bool fillWithBots = true;
    public int minBots = 1;
}
