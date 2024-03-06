using System;
using System.Collections.Generic;
using _game.Prefabs;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using _game.Scripts.UIScripts;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour, IDataPersistence
    {
        [field: SerializeField, Expandable] public TileDatabaseSO TileDatabase { get; private set; }
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private VirtualMouseInput _virtualMouseInput;
        [SerializeField] private RectTransform _virtualCursor;
        [SerializeField] private EditorUIController _editorUI;
        [SerializeField] private LayerMask _collisionCheckRoad;
        [SerializeField] private LayerMask _collisionCheckObject;
        [SerializeField] private LayerMask _collisionCheckArea;
        [SerializeField] private LayerMask _editorRaycast;
        [ColorUsage(true, true), SerializeField]
        private Color _collidingColor, _notCollidingColor, _editingColor;
        [SerializeField] private Material _outlineMat, _tintMat;
        private TileController _activeTile;
        private Layer _activeTileDefaultLayer = Layer.Default;
        private Vector3 _cursorPosition;
        [SerializeField] private int _selectedId;
        [SerializeField] private SerializableDictionary<string, GameObject> _placedTiles = new SerializableDictionary<string, GameObject>();
        [SerializeField] private StudioEventEmitter _tilePlaceSound, _tileDestroySound;
        private bool _isColliding;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[10];
        private Camera _cameraMain;
        [SerializeField] private TileController _startTile, _endTile;
        private TileController _lastDestroyTarget, _lastEditTarget;
        private static readonly int BaseColor = Shader.PropertyToID("_Color");
        public bool EditorViewPressed { get; set; }
        [SerializeField] private Transform _testCollider;

        public static event Action<EditorMode> OnEditorModeChanged;
        public static event Action OnResetObstacles;

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
            _cursorPosition = _controlScheme == ControlScheme.KeyboardMouse ? _mousePosition : _virtualCursor.position;
            _cursorPosition.z = _cameraMain.nearClipPlane + _cameraMain.transform.position.y;

            if (_activeTile)
                _activeTile.Rotate(_rotateInput);
        }

        private enum ControlScheme
        {
            KeyboardMouse,
            Controller
        }
        private ControlScheme _controlScheme = ControlScheme.KeyboardMouse;
        public void OnControlsChange(PlayerInput playerInput)
        {
            switch (_playerInput.currentControlScheme)
            {
                case "Keyboard&Mouse":
                    _controlScheme = ControlScheme.KeyboardMouse;
                    Mouse.current.WarpCursorPosition(_virtualCursor.position);
                    Cursor.visible = true;
                    _virtualCursor.gameObject.SetActive(false);
                    break;
                case "Controller":
                    _controlScheme = ControlScheme.Controller;
                    Cursor.visible = false;
                    _virtualCursor.gameObject.SetActive(true);
                    InputState.Change(_virtualMouseInput.virtualMouse, Mouse.current.position.ReadValue());
                    break;
            }
        }

        public void SetPlaceMode(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) SetEditorMode(EditorMode.Place);
        }

        public void SetEditMode(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) SetEditorMode(EditorMode.Edit);
        }
        public void SetDestroyMode(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) SetEditorMode(EditorMode.Destroy);
        }

        public void CancelTile(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !_activeTile) return;

            SetEditorMode(EditorMode.Place);
            _activeTile.Destroy();
            SetActiveTile(null);
        }

        private Vector2 _mousePosition;
        public void GetMousePosition(InputAction.CallbackContext ctx) { _mousePosition = ctx.ReadValue<Vector2>(); }

        private void HandleEditMode()
        {
            if (_activeTile)
            {
                SetTileToMousePos(_activeTile);

                if (EditorViewPressed)
                {
                    if (_isColliding)
                        _editorUI.DisplayWarning(0);
                    else
                    {
                        _activeTile.SetActiveArrows(false);
                        _activeTile.ChangeCollisionAndRenderLayer((int)_activeTileDefaultLayer);
                        SetActiveTile(null);
                        _tilePlaceSound.Play();
                    }
                }
            }
            else
            {
                Ray ray = _cameraMain.ScreenPointToRay(_cursorPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000, _editorRaycast))
                {
                    SetHighlightMaterialColor(_editingColor);
                    TileController hitTile = hit.transform.GetComponent<TileController>();
                    if (_lastEditTarget != hitTile)
                    {
                        if (_lastEditTarget)
                            _lastEditTarget.ChangeRenderLayer((int)Layer.NormalRender);
                        hitTile.ChangeRenderLayer((int)Layer.PickedUpRender);
                    }

                    _lastEditTarget = hitTile;

                    if (EditorViewPressed)
                    {
                        SetActiveTile(hitTile);
                        hitTile.ChangeCollisionAndRenderLayer((int)Layer.NoCollision);
                        _activeTile.SetActiveArrows(true);
                        _tileDestroySound.Play();
                    }
                }
                else
                {
                    if (_lastEditTarget)
                        _lastEditTarget.ChangeRenderLayer((int)Layer.NormalRender);
                    _lastEditTarget = null;
                }
            }
        }

        private void HandlePlaceMode()
        {
            if (!_activeTile)
                return;

            SetTileToMousePos(_activeTile);

            if (!EditorViewPressed) return;
            if (_isColliding)
            {
                _editorUI.DisplayWarning(0);
                return;
            }
            _activeTile.SetActiveArrows(false);
            _activeTile.ChangeCollisionAndRenderLayer((int)_activeTileDefaultLayer);
            if (_selectedId == 4) //4 is StartTileId
            {
                if (_startTile)
                {
                    DestroyImmediate(_startTile.gameObject);
                    RemoveTileFromList(_startTile.Id);
                }
                _startTile = _activeTile;
            }
            if (_selectedId == 5) //5 is EndTileId
            {
                if (_endTile)
                {
                    DestroyImmediate(_endTile.gameObject);
                    RemoveTileFromList(_endTile.Id);
                }
                _endTile = _activeTile;
            }
            AddTileToList(_activeTile, _selectedId);
            SetActiveTile(Instantiate(GetTileById(_selectedId), _cameraMain.ViewportToWorldPoint(_cursorPosition), _rotation));
            _activeTile.ChangeCollisionAndRenderLayer((int)Layer.NoCollision);
            _tilePlaceSound.Play();
        }

        private float _rotateInput;
        public void Rotate(InputAction.CallbackContext ctx)
        {
            if (!_activeTile) return;
            if (ctx.started)
            {
                if (_activeTileDefaultLayer is Layer.Road or Layer.RoadTrigger)
                    _activeTile.Rotate(90 * ctx.ReadValue<float>());
                if (_activeTileDefaultLayer is Layer.Object or Layer.ObjectTrigger)
                    _rotateInput = ctx.ReadValue<float>();
            }
            if (ctx.canceled)
                _rotateInput = 0;

            _rotation = _activeTile.transform.rotation;
        }

        private static void RoundRotation(TileController t)
        {
            if (t.gameObject.layer is not ((int)Layer.Road or (int)Layer.RoadTrigger)) return;

            Vector3 rotation = t.Rotation.eulerAngles;
            rotation.y = Mathf.RoundToInt(rotation.y / 90) * 90;
            t.Rotation = Quaternion.Euler(rotation);
        }

        private void HandleDestroyMode()
        {
            Ray ray = _cameraMain.ScreenPointToRay(_cursorPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, _editorRaycast))
            {
                SetHighlightMaterialColor(_collidingColor);
                TileController hitTile = hit.transform.GetComponent<TileController>();

                if (_lastDestroyTarget != hitTile)
                {
                    if (_lastDestroyTarget)
                        _lastDestroyTarget.ChangeRenderLayer((int)Layer.NormalRender);
                    hitTile.ChangeRenderLayer((int)Layer.PickedUpRender);
                }

                _lastDestroyTarget = hitTile;

                if (EditorViewPressed)
                {
                    RemoveTileFromList(_lastDestroyTarget.Id);
                    _lastDestroyTarget.Destroy();
                    _tileDestroySound.Play();
                }
            }
            else
            {
                if (_lastDestroyTarget)
                    _lastDestroyTarget.ChangeRenderLayer((int)Layer.NormalRender);
                _lastDestroyTarget = null;
            }
        }

        private TileController GetTileById(int index) { return TileDatabase.AllTiles.Find(data => data.ID == index).Prefab.GetComponent<TileController>(); }

        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            if (!_activeTile)
                return;

            Vector3 worldCenter = _activeTile.transform.TransformPoint(_activeTile.BoxCollider.center);
            Vector3 worldHalfExtents = /*_activeTileTransform.TransformVector*/(_activeTile.BoxCollider.size * 0.45f).Abs();
            LayerMask layerMask = GetActiveTileCollisionCheck(_activeTileDefaultLayer);

            if (_testCollider)
            {
                _testCollider.position = worldCenter;
                _testCollider.localScale = worldHalfExtents * 2;
                _testCollider.rotation = _activeTile.transform.rotation;
            }

            _isColliding = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, _overlapBuffer, _activeTile.transform.rotation, layerMask, QueryTriggerInteraction.Ignore) > 0;
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
            if (!_activeTile)
                return;

            Gizmos.color = Color.red;
            Vector3 worldCenter = _activeTile.transform.TransformPoint(_activeTile.BoxCollider.center);
            Vector3 worldExtents = _activeTile.transform.TransformVector(_activeTile.BoxCollider.size * 0.9f);
            Gizmos.DrawWireCube(worldCenter, worldExtents);
        }

        public void SetEditorMode(int value)
        {
            if (_editorMode == (EditorMode)value) return;

            _editorMode = (EditorMode)value;
            OnEditorModeChanged?.Invoke(_editorMode);

            //Turn off highlight of last placed tile
            if (_lastEditTarget)
                _lastEditTarget.ChangeRenderLayer((int)Layer.NormalRender);
            if (_lastDestroyTarget)
                _lastDestroyTarget.ChangeRenderLayer((int)Layer.NormalRender);

            if (_activeTile)
                _activeTile.Destroy();

            SetActiveTile(null);
            _editorUI.EditorEnabled(_editorMode != EditorMode.Off);
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }

        private void OnGameStateChanged(GameState gameState) { SetEditorMode(gameState == GameState.Editing ? EditorMode.Place : EditorMode.Off); }

        public void CreateTileById(int id)
        {
            SetEditorMode(EditorMode.Place);
            if (_activeTile)
                _activeTile.Destroy();
            if (id == -1) return;
            SetActiveTile(Instantiate(GetTileById(id), _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10), _rotation));
            _selectedId = id;
            _activeTile.ChangeCollisionAndRenderLayer((int)Layer.NoCollision);
        }

        private void SetActiveTile(TileController value)
        {
            _activeTile = value;

            if (_activeTile)
            {
                _activeTileDefaultLayer = (Layer)_activeTile.gameObject.layer;
                RoundRotation(_activeTile);
            }
            else
            {
                _activeTile = null;
            }
        }

        private void AddTileToList(TileController value, int tileId)
        {
            string newId = Guid.NewGuid().ToString();

            value.Id = newId;
            value.TileID = tileId;

            _placedTiles[newId] = value.gameObject;
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

        private void SetTileToMousePos(TileController tile)
        {
            tile.Position = _activeTileDefaultLayer switch
            {
                Layer.Object => _cameraMain.ScreenToWorldPoint(_cursorPosition).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.ObjectTrigger => _cameraMain.ScreenToWorldPoint(_cursorPosition).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.RoadTrigger => _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.Road => _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10),
                _ => tile.Position
            };
        }

        public bool CanStart() => _startTile && _endTile;

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }

        public void ResetObstacles() { OnResetObstacles?.Invoke(); }

        public void LoadLevel(LevelData data)
        {
            foreach (TileData tileData in data.TileMap)
            {
                TileController newTile = Instantiate(GetTileById(tileData.ID), tileData.Position, tileData.Rotation).GetComponent<TileController>();
                AddTileToList(newTile, tileData.ID);

                newTile.SetActiveArrows(false);
                if (tileData.ID == 4) //4 is StartTileId
                    _startTile = newTile;
                else if (tileData.ID == 5) //5 is EndTileId
                    _endTile = newTile;
            }
        }

        public void SaveLevel(LevelData data)
        {
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
