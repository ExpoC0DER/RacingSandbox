using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace _game.Prefabs
{
    [CreateAssetMenu(menuName = "Scriptable Objects/TileDatabaseSO")]
    // ReSharper disable once InconsistentNaming
    public class TileDatabaseSO : ScriptableObject
    {
        public List<TileProperties> AllTiles
        {
            get
            {
                List<TileProperties> allTiles = new List<TileProperties>();
                allTiles.AddRange(BasicTiles);
                allTiles.AddRange(ControlTiles);
                allTiles.AddRange(ObstacleTiles);
                return allTiles;
            }
        }

        [field: SerializeField] public List<TileProperties> BasicTiles { get; private set; }
        [field: SerializeField] public List<TileProperties> ControlTiles { get; private set; }
        [field: SerializeField] public List<TileProperties> ObstacleTiles { get; private set; }

        private void OnValidate()
        {
            Dictionary<int, int> usedIDs = new Dictionary<int, int>();
            foreach (TileProperties tileData in AllTiles)
            {
                if (usedIDs.ContainsKey(tileData.ID))
                    usedIDs[tileData.ID]++;
                else
                    usedIDs.Add(tileData.ID, 1);

                tileData.ValidID = true;
            }
            foreach (TileProperties tileData in AllTiles)
            {
                if (usedIDs[tileData.ID] > 1)
                    tileData.ValidID = false;
            }
        }
    }

    [Serializable]
    public class TileProperties
    {
        [field: SerializeField] public string Name { get; private set; }
        [SerializeField, ReadOnly, AllowNesting, HideIf("ValidID")]
        private string _errorMessage = "ID already in use!";
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField, ShowAssetPreview]
        public GameObject Prefab { get; private set; }
        [field: SerializeField, ShowAssetPreview]
        public Sprite Sprite { get; private set; }

        public bool ValidID { get; set; } = true;
    }
}
