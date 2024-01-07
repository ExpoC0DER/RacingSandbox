using System;
using System.Collections.Generic;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using _game.Scripts.UIScripts;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;
using Object = System.Object;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour, IDataPersistence
    {
        [SerializeField] private GameObject[] _tiles;
        [SerializeField] private EditorUIController _editorUI;
        [FormerlySerializedAs("_collisionCheck")]
        [SerializeField] private LayerMask _collisionCheckRoad;
        [FormerlySerializedAs("_collisionCheckObstacle")]
        [SerializeField] private LayerMask _collisionCheckObject;
        [SerializeField] private LayerMask _collisionCheckArea;
        [SerializeField] private LayerMask _editorRaycast;
        [SerializeField] private Transform _test;
        [ColorUsage(true, true), SerializeField]
        private Color _collidingColor, _notCollidingColor, _editingColor;
        [SerializeField] private Material _outlineMat, _tintMat;
        private Transform _activeTileTransform;
        private BoxCollider _activeTileCollider;
        private Layer _activeTileDefaultLayer = Layer.Default;
        private Vector3 _mousePos;
        [SerializeField] private int _selectedId;
        [SerializeField] private string _selectedTileControllerId;
        [SerializeField] private SerializableDictionary<string, GameObject> _placedTiles = new SerializableDictionary<string, GameObject>();
        [SerializeField] private StudioEventEmitter _tilePlaceSound, _tileDestroySound;
        private bool _isColliding;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[10];
        private Camera _cameraMain;
        [SerializeField] private TileController _startTile, _endTile;
        private GameObject _lastDestroyTarget, _lastEditTarget;
        private static readonly int BaseColor = Shader.PropertyToID("_Color");
        public bool EditorViewPressed { get; set; }

        public static event Action<EditorMode> OnEditorModeChanged;

        private void Awake() { _cameraMain = Camera.main; }

        private void Update()
        {
            HandleInput();

            if (_editorMode == EditorMode.Place)
                HandlePlaceMode();
            if (_editorMode == EditorMode.Destroy)
                HandleDestroyMode();
            if (_editorMode == EditorMode.Edit)
                HandleEditMode();

            EditorViewPressed = false;
        }

        private void HandleInput()
        {
            _mousePos = Input.mousePosition;
            _mousePos.z = _cameraMain.nearClipPlane + _cameraMain.transform.position.y;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetEditorMode(EditorMode.Place);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetEditorMode(EditorMode.Edit);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetEditorMode(EditorMode.Destroy);
            }
        }

        private void HandleEditMode()
        {
            if (_activeTileTransform)
            {
                SetTileToMousePos(_activeTileTransform);

                if (Input.GetKeyDown(KeyCode.R))
                {
                    _activeTileTransform.transform.Rotate(new(0, 90, 0));
                    _rotation = _activeTileTransform.transform.rotation;
                }
                if (EditorViewPressed)
                {
                    if (_isColliding)
                        _editorUI.DisplayWarning(0);
                    else
                    {
                        if (_activeTileTransform.TryGetComponent(out TileController tileController))
                            tileController.SetActiveArrows(false);
                        ChangeCollisionAndRenderLayer(_activeTileTransform.gameObject, (int)_activeTileDefaultLayer);
                        SetActiveTile((Transform)null);
                        _tilePlaceSound.Play();
                    }
                }
            }
            else
            {
                Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000, _editorRaycast))
                {
                    SetHighlightMaterialColor(_editingColor);
                    GameObject hitGameObject = hit.transform.gameObject;
                    if (_lastEditTarget != hitGameObject)
                    {
                        ChangeRenderLayer(_lastEditTarget, (int)Layer.NormalRender);
                        ChangeRenderLayer(hitGameObject, (int)Layer.PickedUpRender);
                    }

                    _lastEditTarget = hitGameObject;

                    if (EditorViewPressed)
                    {
                        SetActiveTile(hitGameObject);
                        ChangeCollisionAndRenderLayer(hitGameObject, (int)Layer.NoCollision);
                        if (_activeTileTransform.TryGetComponent(out TileController tileController))
                            tileController.SetActiveArrows(true);
                        _tileDestroySound.Play();
                    }
                }
                else
                {
                    if (_lastEditTarget)
                        ChangeRenderLayer(_lastEditTarget, (int)Layer.NormalRender);
                    _lastEditTarget = null;
                }
            }
        }

        private void HandlePlaceMode()
        {
            if (!_activeTileTransform)
                return;

            SetTileToMousePos(_activeTileTransform);

            if (Input.GetKeyDown(KeyCode.R))
            {
                _activeTileTransform.transform.Rotate(new(0, 90, 0));
                _rotation = _activeTileTransform.transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(_activeTileTransform.gameObject);
                SetActiveTile((Transform)null);
            }


            if (!EditorViewPressed) return;
            if (_isColliding)
            {
                _editorUI.DisplayWarning(0);
                return;
            }
            ChangeCollisionAndRenderLayer(_activeTileTransform.gameObject, (int)_activeTileDefaultLayer);
            if (_activeTileTransform.TryGetComponent(out TileController tileController))
                tileController.SetActiveArrows(false);
            if (_selectedId == 4) //4 is StartTileId
            {
                if (_startTile)
                {
                    DestroyImmediate(_startTile.gameObject);
                    RemoveTileFromList(_startTile.Id);
                }
                _startTile = tileController;
            }
            if (_selectedId == 5) //5 is EndTileId
            {
                if (_endTile)
                {
                    DestroyImmediate(_endTile.gameObject);
                    RemoveTileFromList(_endTile.Id);
                }
                _endTile = tileController;
            }
            AddTileToList(_activeTileTransform.gameObject, _selectedId);
            SetActiveTile(Instantiate(_tiles[_selectedId], _cameraMain.ViewportToWorldPoint(Input.mousePosition), _rotation).transform);
            ChangeCollisionAndRenderLayer(_activeTileTransform.gameObject, (int)Layer.NoCollision);
            _tilePlaceSound.Play();
        }

        private void HandleDestroyMode()
        {
            Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, _editorRaycast))
            {
                SetHighlightMaterialColor(_collidingColor);
                GameObject hitObject = hit.transform.gameObject;

                if (_lastDestroyTarget != hitObject)
                {
                    ChangeRenderLayer(_lastDestroyTarget, (int)Layer.NormalRender);
                    ChangeRenderLayer(hitObject, (int)Layer.PickedUpRender);
                }

                _lastDestroyTarget = hitObject;

                if (EditorViewPressed)
                {
                    if (_lastDestroyTarget.TryGetComponent(out TileController tileController))
                        _selectedTileControllerId = tileController.Id;
                    Destroy(_lastDestroyTarget);
                    RemoveTileFromList(_selectedTileControllerId);
                    _tileDestroySound.Play();
                }
            }
            else
            {
                if (_lastDestroyTarget)
                    ChangeRenderLayer(_lastDestroyTarget, (int)Layer.NormalRender);
                _lastDestroyTarget = null;
            }
        }


        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            if (!_activeTileTransform)
                return;

            Vector3 worldCenter = _activeTileTransform.TransformPoint(_activeTileCollider.center);
            Vector3 worldHalfExtents = _activeTileTransform.TransformVector(_activeTileCollider.size * 0.45f).Abs();
            LayerMask layerMask = GetActiveTileCollisionCheck(_activeTileDefaultLayer);

            _isColliding = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, _overlapBuffer, Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore) > 0;
            if (_editorMode == EditorMode.Edit)
                SetHighlightMaterialColor(_isColliding ? _collidingColor : _editingColor);
            if (_editorMode == EditorMode.Place)
                SetHighlightMaterialColor(_isColliding ? _collidingColor : _notCollidingColor);
        }

        private LayerMask GetActiveTileCollisionCheck(Layer layer)
        {
            return layer switch
            {
                Layer.Object => _collisionCheckObject,
                Layer.ObjectTrigger => _collisionCheckObject,
                Layer.Road => _collisionCheckRoad,
                Layer.RoadTrigger => _collisionCheckArea,
                _ => (int)Layer.NoCollision
            };
        }

        private void OnDrawGizmos()
        {
            if (!_activeTileTransform)
                return;

            Gizmos.color = Color.red;
            Vector3 worldCenter = _activeTileTransform.TransformPoint(_activeTileCollider.center);
            Vector3 worldExtents = _activeTileTransform.TransformVector(_activeTileCollider.size * 0.9f);
            Gizmos.DrawWireCube(worldCenter, worldExtents);
        }

        private static void ChangeCollisionAndRenderLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                if (IsDefaultLayer(layer))
                {
                    ChangeRenderLayer(child.gameObject, (int)Layer.NormalRender);
                    ChangeCollisionLayer(child.gameObject, (int)Layer.Default);
                }
                else
                {
                    ChangeRenderLayer(child.gameObject, (int)Layer.PickedUpRender);
                    ChangeCollisionLayer(child.gameObject, (int)Layer.NoCollision);
                }
            }
        }

        private static void ChangeCollisionLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.layer is (int)Layer.Default or (int)Layer.NoCollision)
                    child.gameObject.layer = layer;
                ChangeRenderLayer(child.gameObject, layer);
            }
        }

        private static void ChangeRenderLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.layer is (int)Layer.NormalRender or (int)Layer.PickedUpRender)
                    child.gameObject.layer = layer;
                ChangeRenderLayer(child.gameObject, layer);
            }
        }

        public void SetEditorMode(int value)
        {
            if (_editorMode == (EditorMode)value) return;

            _editorMode = (EditorMode)value;
            OnEditorModeChanged?.Invoke(_editorMode);

            //Turn off highlight of last placed tile
            if (_lastEditTarget)
                ChangeRenderLayer(_lastEditTarget, (int)Layer.NormalRender);
            if (_lastDestroyTarget)
                ChangeRenderLayer(_lastDestroyTarget, (int)Layer.NormalRender);

            if (_activeTileTransform)
                Destroy(_activeTileTransform.gameObject);

            SetActiveTile((Transform)null);
            _editorUI.EditorEnabled(_editorMode != EditorMode.Off);
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }

        private void OnGameStateChanged(GameState gameState) { SetEditorMode(gameState == GameState.Editing ? EditorMode.Place : EditorMode.Off); }

        public void CreateTileById(int id)
        {
            SetEditorMode(EditorMode.Place);
            if (_activeTileTransform)
                Destroy(_activeTileTransform.gameObject);
            if (id == -1) return;
            SetActiveTile(Instantiate(_tiles[id], _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), _rotation).transform);
            _selectedId = id;
            ChangeCollisionAndRenderLayer(_activeTileTransform.gameObject, (int)Layer.NoCollision);
        }
        private void SetActiveTile(GameObject value) { SetActiveTile(value.transform); }
        private void SetActiveTile(Transform value)
        {
            _activeTileTransform = value;

            if (_activeTileTransform)
            {
                if (_activeTileTransform.TryGetComponent(out TileController tileController))
                    _selectedTileControllerId = tileController.Id;
                _activeTileCollider = _activeTileTransform.GetComponent<BoxCollider>();
                _activeTileDefaultLayer = (Layer)_activeTileTransform.gameObject.layer;
            }
            else
            {
                _activeTileCollider = null;
            }
        }

        private void AddTileToList(GameObject value, int tileId)
        {
            string newId = Guid.NewGuid().ToString();
            if (value.TryGetComponent(out TileController tileController))
            {
                tileController.Id = newId;
                tileController.TileID = tileId;
            }

            _placedTiles[newId] = value;
        }

        private void RemoveTileFromList(string id)
        {
            if (_placedTiles.ContainsKey(id))
                _placedTiles.Remove(id);
        }

        private void SetHighlightMaterialColor(Color color)
        {
            _outlineMat.SetColor(BaseColor, color);
            _tintMat.SetColor(BaseColor, color);
        }

        private void SetTileToMousePos(Transform tile)
        {
            tile.position = _activeTileDefaultLayer switch
            {
                Layer.Object => _cameraMain.ScreenToWorldPoint(_mousePos).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.ObjectTrigger => _cameraMain.ScreenToWorldPoint(_mousePos).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.RoadTrigger => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.Road => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10),
                _ => tile.position
            };
        }

        private static bool IsDefaultLayer(int layer) { return layer is (int)Layer.Road or (int)Layer.Object or (int)Layer.RoadTrigger or (int)Layer.ObjectTrigger or (int)Layer.Default; }

        public bool CanStart() => _startTile && _endTile;

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }

        public void LoadLevel(LevelData data)
        {
            foreach (TileData tileData in data.TileMap)
            {
                GameObject newTile = Instantiate(_tiles[tileData.ID], tileData.Position, tileData.Rotation);
                AddTileToList(newTile, tileData.ID);

                if (!newTile.TryGetComponent(out TileController tileController)) continue;

                tileController.SetActiveArrows(false);
                if (tileData.ID == 4) //4 is StartTileId
                    _startTile = tileController;
                else if (tileData.ID == 5) //5 is EndTileId
                    _endTile = tileController;
            }
        }

        [SerializeField] private string _name;
        public void SaveLevel(LevelData data)
        {
            data.Name = _name;
            List<TileData> newTileMap = new();
            foreach (KeyValuePair<string, GameObject> pair in _placedTiles)
            {
                newTileMap.Add(new TileData(pair.Value.GetComponent<TileController>().TileID, pair.Value.transform.position, pair.Value.transform.rotation));
            }
            data.TileMap = newTileMap;
        }
    }

    [Serializable]
    public struct TileData
    {
        public int ID;
        public Vector3 Position;
        public Quaternion Rotation;

        public TileData(int id, Vector3 position, Quaternion rotation)
        {
            ID = id;
            Position = position;
            Rotation = rotation;
        }
    }

    public enum EditorMode
    {
        Place = 0,
        Destroy = 1,
        Off = 2,
        Edit = 3
    }

    public enum Layer
    {
        Default = 0,
        Car = 3,
        NoCollision = 6,
        Road = 7,
        Object = 8,
        RoadTrigger = 9,
        ObjectTrigger = 10,
        NormalRender = 11,
        PickedUpRender = 12
    }
}
