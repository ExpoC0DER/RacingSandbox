using System;
using System.Collections.Generic;

namespace _game.Scripts.Saving
{
    [Serializable]
    public class LevelData
    {
        public string Name;
        public List<TileData> TileMap;

        public LevelData()
        {
            Name = "NewLevel";
            TileMap = new();
        }
    }
}
