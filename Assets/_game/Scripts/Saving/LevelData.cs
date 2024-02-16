using System;
using System.Collections.Generic;
using _game.Scripts.UIScripts;
using UnityEngine;

namespace _game.Scripts.Saving
{
    [Serializable]
    public class LevelData
    {
        public string Name;
        public Dictionary<Trophy, int> TrophyTimes;
        public List<TileData> TileMap;

        public LevelData()
        {
            Name = null;
            TileMap = new();
            TrophyTimes = new Dictionary<Trophy, int>
            {
                { Trophy.Gold, 0 },
                { Trophy.Silver, 0 },
                { Trophy.Bronze, 0 }
            };
        }

        public LevelData(string name)
        {
            Name = name;
            TileMap = new();
            TrophyTimes = new Dictionary<Trophy, int>
            {
                { Trophy.Gold, 0 },
                { Trophy.Silver, 0 },
                { Trophy.Bronze, 0 }
            };
        }
    }
}
