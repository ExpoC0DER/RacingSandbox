using System;
using System.Collections.Generic;
using System.Linq;
using _game.Prefabs;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using _game.Scripts.UIScripts;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour, IDataPersistence
    {
        private enum ControlScheme
        {
            KeyboardMouse,
            Controller
        }

        [field: Header("References")]
        [field: SerializeField, Expandable] public TileDatabaseSO TileDatabase { get; private set; }
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private VirtualMouseInput _virtualMouseInput;
        [SerializeField] private RectTransform _virtualCursor;
        [SerializeField] private EditorUIController _editorUI;
        [SerializeField] private StudioEventEmitter _tilePlaceSound, _tileDestroySound;
        [SerializeField] private RectTransform _rotationKey;
        [SerializeField] private RectTransform _rotationKeyParent;
        [SerializeField] private EditorUIController _uiController;
        [SerializeField] private Transform _testCollider;

        [Header("Settings")]
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private LayerMask _editorRaycast;
        [ColorUsage(true, true), SerializeField]
        private Color _collidingColor, _notCollidingColor, _editingColor;
        [SerializeField] private Material _outlineMat, _tintMat;
        [SerializeField] private int[] _startTileIds, _endTileIds, _lapTileIds;

        [Header("Debug")]
        [SerializeField, ReadOnly] private SerializableDictionary<string, GameObject> _placedTiles = new SerializableDictionary<string, GameObject>();
        [SerializeField, ReadOnly] private TileController _startTile, _endTile, _lapTile;
        [SerializeField, ReadOnly] private TileController _activeTile;

        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private Camera _cameraMain;
        private TileController _lastDestroyTarget, _lastEditTarget;
        private ControlScheme _controlScheme = ControlScheme.KeyboardMouse;
        private Vector3 _cursorPosition;
        private Vector2 _mousePosition;
        private float _rotateInput;
        private static readonly int BaseColor = Shader.PropertyToID("_Color");
        //private bool _canPlace = true;
        private bool _isMouseOverUI;
        private bool _clickHeld;
        private RaycastHit[] _eyeDropperHits = new RaycastHit[5];

        public bool IsLapping { get { return _lapTile; } }

        public static event Action<EditorMode> OnEditorModeChanged;
        public static event Action OnResetObstacles;

        private void Awake() { _cameraMain = Camera.main; }

        private void Update()
        {
            HandleInput();

            switch (_editorMode)
            {
                case EditorMode.Place:
                    HandlePlaceModeHighlight();
                    break;
                case EditorMode.Edit:
                    HandleEditModeHighlight();
                    break;
                case EditorMode.Destroy:
                    HandleDestroyModeHighlight();
                    break;
            }
        }

        private void FixedUpdate()
        {
            //Handle edit modes in fixed update as they require collision info
            if (_clickHeld)
                switch (_editorMode)
                {
                    case EditorMode.Place:
                        TilePlace();
                        break;
                    case EditorMode.Destroy:
                        TileDestroy();
                        break;
                }

            if (_activeTile && !_uiController.PopupOpen)
            {
                SetTileToMousePos(_activeTile);
                //_rotationSprite.position = new Vector3(_activeTile.Position.x, 45, _activeTile.Position.z);
            }

            //EditorViewPressed = false;
        }

        private void HandleInput()
        {
            _isMouseOverUI = EventSystem.current.IsPointerOverGameObject();

            _cursorPosition = _controlScheme == ControlScheme.KeyboardMouse ? _mousePosition : _virtualCursor.position;
            _cursorPosition.z = _cameraMain.nearClipPlane + _cameraMain.transform.position.y;

            if (!_activeTile)
            {
                _rotationKey.gameObject.SetActive(false);
                return;
            }

            //Smoothly rotate tile
            _activeTile.Rotate(_rotateInput * _rotationSpeed * Time.deltaTime);

            //Set rotation key binds scale to work properly with different screen sizes
            float canvasScale = transform.parent.localScale.x;
            _rotationKeyParent.localScale = Vector3.one * 1f / canvasScale;
            _rotationKey.localScale = Vector3.one * canvasScale;

            //Show rotation key binds and move them to tile position
            _rotationKey.gameObject.SetActive(true);
            _rotationKeyParent.anchoredPosition = _cameraMain.WorldToScreenPoint(_activeTile.Position);
        }

        private void HandleEditModeHighlight()
        {
            if (_activeTile)
            {
                SetHighlightMaterialColor(_activeTile.IsColliding ? _collidingColor : _editingColor);
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
                }
                else
                {
                    if (_lastEditTarget)
                        _lastEditTarget.ChangeRenderLayer((int)Layer.NormalRender);
                    _lastEditTarget = null;
                }
            }
        }

        private void TileEdit()
        {
            if (_activeTile)
            {
                //SetTileToMousePos(_activeTile);
                if (_activeTile.IsColliding)
                {
                    _editorUI.DisplayWarning(0);
                }
                else
                {
                    _activeTile.Place();
                    SetActiveTile(null);
                    _tilePlaceSound.Play();
                }
            }
            else if (_lastEditTarget)
            {
                SetActiveTile(_lastEditTarget);
                _activeTile.PickUp();
                _tileDestroySound.Play();
            }
        }

        private void HandlePlaceModeHighlight()
        {
            if (!_activeTile)
                return;

            SetHighlightMaterialColor(_activeTile.IsColliding ? _collidingColor : _notCollidingColor);
        }

        private void TilePlace()
        {
            if (!_activeTile)
                return;

            if (_activeTile.IsColliding)
            {
                _editorUI.DisplayWarning(0);
                return;
            }

            // Ray ray = _cameraMain.ScreenPointToRay(_cursorPosition);
            // RaycastHit[] hits = Physics.RaycastAll(ray, 1000, _editorRaycast);
            //
            // string hitTargets = "";
            // foreach (RaycastHit hit in hits)
            // {
            //     if (!hit.transform.GetComponent<TileController>().IsSelected)
            //     {
            //         _canPlace = false;
            //         hitTargets += hit.transform.name;
            //     }
            // }
            // if (!string.IsNullOrEmpty(hitTargets))
            //     print(hitTargets);

            //if (!_canPlace) return;

            _activeTile.Place();

            if (_startTileIds.Any(startTileId => _activeTile.TileID == startTileId))
            {
                if (_startTile)
                {
                    DestroyImmediate(_startTile.gameObject);
                    RemoveTileFromList(_startTile.Id);
                    _startTile = null;
                }
                if (_lapTile)
                {
                    DestroyImmediate(_lapTile.gameObject);
                    RemoveTileFromList(_lapTile.Id);
                    _lapTile = null;
                }
                _startTile = _activeTile;
            }

            if (_endTileIds.Any(endTileId => _activeTile.TileID == endTileId))
            {
                if (_endTile)
                {
                    DestroyImmediate(_endTile.gameObject);
                    RemoveTileFromList(_endTile.Id);
                    _endTile = null;
                }
                if (_lapTile)
                {
                    DestroyImmediate(_lapTile.gameObject);
                    RemoveTileFromList(_lapTile.Id);
                    _lapTile = null;
                }
                _endTile = _activeTile;
            }

            if (_lapTileIds.Any(lapTileId => _activeTile.TileID == lapTileId))
            {
                if (_lapTile)
                {
                    DestroyImmediate(_lapTile.gameObject);
                    RemoveTileFromList(_lapTile.Id);
                    _lapTile = null;
                }
                if (_startTile)
                {
                    DestroyImmediate(_startTile.gameObject);
                    RemoveTileFromList(_startTile.Id);
                    _startTile = null;
                }
                if (_endTile)
                {
                    DestroyImmediate(_endTile.gameObject);
                    RemoveTileFromList(_endTile.Id);
                    _endTile = null;
                }
                _lapTile = _activeTile;
            }

            AddTileToList(_activeTile);
            SetActiveTile(InstantiateNewTile(_activeTile.TileID));
            _activeTile.PickUp();
            _tilePlaceSound.Play();
        }

        private static void RoundRotation(TileController t)
        {
            if (t.gameObject.layer is not ((int)Layer.Road or (int)Layer.RoadTrigger)) return;

            Vector3 rotation = t.Rotation.eulerAngles;
            rotation.y = Mathf.RoundToInt(rotation.y / 90) * 90;
            t.Rotation = Quaternion.Euler(rotation);
        }

        private void HandleDestroyModeHighlight()
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
            }
            else
            {
                if (_lastDestroyTarget)
                    _lastDestroyTarget.ChangeRenderLayer((int)Layer.NormalRender);
                _lastDestroyTarget = null;
            }
        }

        private void TileEyedropper()
        {
            Ray ray = _cameraMain.ScreenPointToRay(_cursorPosition);
            Physics.RaycastNonAlloc(ray, _eyeDropperHits, 1000, _editorRaycast);

            foreach (RaycastHit hit in _eyeDropperHits)
            {
                if (!hit.transform || !hit.transform.TryGetComponent(out TileController hitTile))
                    continue;

                if (hitTile.IsSelected)
                    continue;

                CreateTileById(hitTile.TileID);
                break;
            }
        }

        private void TileDestroy()
        {
            if (!_lastDestroyTarget) return;

            RemoveTileFromList(_lastDestroyTarget.Id);
            _lastDestroyTarget.Destroy();
            _tileDestroySound.Play();
        }

        private TileController GetTilePrefabById(int index) { return TileDatabase.AllTiles.Find(data => data.ID == index).Prefab.GetComponent<TileController>(); }

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
            SetActiveTile(InstantiateNewTile(id));
            _activeTile.ChangeRenderLayer((int)Layer.PickedUpRender);
        }

        private TileController InstantiateNewTile(int tileId)
        {
            TileController newTile = Instantiate(GetTilePrefabById(tileId), _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10), _rotation);
            SetTileToMousePos(newTile);
            newTile.TileID = tileId;
            return newTile;
        }

        private void SetActiveTile(TileController value)
        {
            if (value != null)
            {
                _activeTile = value;
                _activeTile.IsSelected = true;
                RoundRotation(_activeTile);
            }
            else
            {
                _activeTile = null;
            }
        }

        private void AddTileToList(TileController value)
        {
            string newId = Guid.NewGuid().ToString();
            value.Id = newId;
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
            if (_activeTile)
                tile.Rb.MovePosition(_activeTile.gameObject.layer switch
                {
                    (int)Layer.Object => _cameraMain.ScreenToWorldPoint(_cursorPosition).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                    (int)Layer.ObjectTrigger => _cameraMain.ScreenToWorldPoint(_cursorPosition).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                    (int)Layer.RoadTrigger => _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                    (int)Layer.Road => _cameraMain.ScreenToWorldPoint(_cursorPosition).RoundToMultiple(10),
                    _ => tile.Position
                });
        }

        public bool CanStart() => (_startTile && _endTile) || _lapTile;

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }

        public void ResetObstacles() { OnResetObstacles?.Invoke(); }

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

        public void Rotate(InputAction.CallbackContext ctx)
        {
            if (!_activeTile) return;
            if (ctx.started)
            {
                if (_activeTile.gameObject.layer is (int)Layer.Road or (int)Layer.RoadTrigger)
                    _activeTile.Rotate(90 * ctx.ReadValue<float>());
                if (_activeTile.gameObject.layer is (int)Layer.Object or (int)Layer.ObjectTrigger)
                    _rotateInput = ctx.ReadValue<float>();
            }
            if (ctx.canceled)
                _rotateInput = 0;

            _rotation = _activeTile.transform.rotation;
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

        public void UseEyedropper(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) TileEyedropper();
        }

        public void GetMousePosition(InputAction.CallbackContext ctx) { _mousePosition = ctx.ReadValue<Vector2>(); }

        public void CancelTile(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !_activeTile) return;

            // SetEditorMode(EditorMode.Place);
            // _activeTile.Destroy();
            // SetActiveTile(null);
        }

        public void LeftClick(InputAction.CallbackContext ctx)
        {
            if (ctx.started && !_isMouseOverUI)
            {
                switch (_editorMode)
                {
                    case EditorMode.Place:
                        TilePlace();
                        break;
                    case EditorMode.Edit:
                        TileEdit();
                        break;
                    case EditorMode.Destroy:
                        TileDestroy();
                        break;
                }
            }
            if (ctx.performed && !_isMouseOverUI)
            {
                _clickHeld = true;
            }
            if (ctx.canceled)
            {
                if (_clickHeld && _editorMode == EditorMode.Edit)
                    TileEdit();
                _clickHeld = false;
            }
        }

        public void LoadLevel(LevelData data)
        {
            foreach (TileData tileData in data.TileMap)
            {
                TileController newTile = Instantiate(GetTilePrefabById(tileData.ID), tileData.Position, tileData.Rotation).GetComponent<TileController>();
                newTile.TileID = tileData.ID;
                AddTileToList(newTile);

                newTile.SetActiveArrows(false);

                if (_startTileIds.Any(startTileId => tileData.ID == startTileId))
                    _startTile = newTile;

                if (_endTileIds.Any(endTileId => tileData.ID == endTileId))
                    _endTile = newTile;

                if (_lapTileIds.Any(lapTileId => tileData.ID == lapTileId))
                    _lapTile = newTile;
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

        // private void SetCollisionMaterialColor()
        // {
        //     if (!_activeTile)
        //         return;
        //
        //     // Vector3 worldCenter = _activeTile.transform.TransformPoint(_activeTile.BoxCollider.center);
        //     // Vector3 worldHalfExtents = /*_activeTileTransform.TransformVector*/(_activeTile.BoxCollider.size * 0.45f).Abs();
        //     // LayerMask layerMask = GetActiveTileCollisionCheck(_activeTileDefaultLayer);
        //     //
        //     // if (_testCollider)
        //     // {
        //     //     _testCollider.position = worldCenter;
        //     //     _testCollider.localScale = worldHalfExtents * 2;
        //     //     _testCollider.rotation = _activeTile.transform.rotation;
        //     // }
        //
        //     //_isColliding = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, _overlapBuffer, _activeTile.transform.rotation, layerMask, QueryTriggerInteraction.Ignore) > 0;
        //     //_isColliding = _activeTile.IsColliding;
        //
        //     if (_editorMode == EditorMode.Edit)
        //         SetHighlightMaterialColor(_activeTile.IsColliding ? _collidingColor : _editingColor);
        //     if (_editorMode == EditorMode.Place)
        //         SetHighlightMaterialColor(_activeTile.IsColliding ? _collidingColor : _notCollidingColor);
        // }
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
        PickedUpRender = 12,
        Tile = 13
    }
}
