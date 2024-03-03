using System.Collections.Generic;
using _game.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class TilesTab : MonoBehaviour
    {
        [SerializeField] private TilePlacing _tilePlacing;
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Transform _content;
        [SerializeField] private TileItemToggle _prefab;

        [SerializeField] private TileList _tileList;

        private enum TileList
        {
            Basic,
            Snow,
            Control,
            Obstacle,
            Decorations
        }

        private List<TileProperties> GetTileList(TileList tileList)
        {
            return tileList switch
            {
                TileList.Basic => _tilePlacing.TileDatabase.BasicTiles,
                TileList.Snow => _tilePlacing.TileDatabase.SnowTiles,
                TileList.Control => _tilePlacing.TileDatabase.ControlTiles,
                TileList.Obstacle => _tilePlacing.TileDatabase.Obstacles,
                TileList.Decorations => _tilePlacing.TileDatabase.Decorations,
                _ => null
            };
        }


        private void Start()
        {
            foreach (TileProperties tileProperties in GetTileList(_tileList))
            {
                TileItemToggle newTileToggle = Instantiate(_prefab, _content);
                newTileToggle.TilePlacing = _tilePlacing;
                newTileToggle.TileId = tileProperties.ID;
                newTileToggle.TileName = tileProperties.Name;
                newTileToggle.Sprite = tileProperties.Sprite;
                newTileToggle.ToggleGroup = _toggleGroup;
            }
        }
    }
}
