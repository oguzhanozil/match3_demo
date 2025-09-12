using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int width = 8;
    public int height = 8;
    public int moves = 15;
    public int targetScore = 1000;

    public enum GoalType { Score, Collect }
    public GoalType goalType = GoalType.Score;
    public int goalAmount = 1000; // for Score or collect count

    [System.Serializable]
    public enum ObstacleType { None, Locked, Jelly, BombTimed }

    [System.Serializable]
    public class ObstacleEntry
    {
        public int x;
        public int y;
        public ObstacleType type;
        public int jellyLayers = 1;   // for jelly
        public int bombTimer = 5;     // for timed bomb (moves)
    }

    public List<ObstacleEntry> obstacles = new List<ObstacleEntry>();

    // optional: list of candy types to collect
    public List<CandyType> collectTypes = new List<CandyType>();
}