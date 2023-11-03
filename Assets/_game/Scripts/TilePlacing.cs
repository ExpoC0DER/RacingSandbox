using System;
using _game.Scripts.UIScripts;
using UnityEngine;
using FMOD;
using FMODUnity;
using UnityEngine.EventSystems;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour
    {
        [SerializeField] private GameObject[] _tiles;
        [SerializeField] private EditorUIController _editorUI;
        [SerializeField] private LayerMask _collisionCheck;
        [SerializeField] private LayerMask _collisionCheckObstacle;
        [SerializeField] private LayerMask _collisionCheckRaycast;
        [ColorUsage(true, true), SerializeField]
        private Color _collidingColor, _notCollidingColor, _editingColor;
        [SerializeField] private Material _previewMaterial;
        private Transform _activeTileTransform;
        private BoxCollider _activeTileCollider;
        private Vector3 _mousePos;
        [SerializeField] private int _selectedId;
        [SerializeField] private StudioEventEmitter _tilePlaceSound, _tileDestroySound;
        private bool _isColliding;
        private TileType _tileType;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[1];
        private Camera _cameraMain;
        private GameObject _startTile, _endTile;
        private GameObject _lastDestroyTarget, _lastEditTarget;
        private static readonly int BaseColor = Shader.PropertyToID("_baseColor");
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
                        ChangeLayer(_activeTileTransform.gameObject, 8);
                        SetActiveTile(null);
                        _tilePlaceSound.Play();
                    }
                }
            }
            else
            {
                Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _previewMaterial.SetColor(BaseColor, _editingColor);
                    Transform hitTransform = GetTransformParent(hit.transform);
                    if (_lastEditTarget && _lastEditTarget != hitTransform.gameObject)
                        ChangeLayer(_lastEditTarget, 8);
                    _lastEditTarget = hitTransform.gameObject;
                    ChangeLayer(_lastEditTarget, 6);
                    if (EditorViewPressed)
                    {
                        SetActiveTile(hitTransform);
                        if (_activeTileTransform.TryGetComponent(out TileController tileController))
                            tileController.SetActiveArrows(true);
                        _tileDestroySound.Play();
                    }
                }
                else
                {
                    if (_lastEditTarget)
                        ChangeLayer(_lastEditTarget, 8);
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
                SetActiveTile(null);
            }


            if (!EditorViewPressed) return;
            if (_isColliding)
            {
                _editorUI.DisplayWarning(0);
                return;
            }
            ChangeLayer(_activeTileTransform.gameObject, 8);
            if (_activeTileTransform.TryGetComponent(out TileController tileController))
                tileController.SetActiveArrows(false);
            if (_selectedId == 4) //4 is StartTileId
            {
                DestroyImmediate(_startTile);
                _startTile = _activeTileTransform.gameObject;
            }
            if (_selectedId == 5) //5 is EndTileId
            {
                DestroyImmediate(_endTile);
                _endTile = _activeTileTransform.gameObject;
            }
            SetActiveTile(Instantiate(_tiles[_selectedId], _cameraMain.ViewportToWorldPoint(Input.mousePosition), _rotation).transform);
            ChangeLayer(_activeTileTransform.gameObject, 6);
            _tilePlaceSound.Play();
        }

        private void HandleDestroyMode()
        {
            Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _previewMaterial.SetColor(BaseColor, _collidingColor);
                Transform hitTransform = GetTransformParent(hit.transform);
                if (_lastDestroyTarget && _lastDestroyTarget != hitTransform.gameObject)
                    ChangeLayer(_lastDestroyTarget, 8);
                _lastDestroyTarget = hitTransform.gameObject;
                ChangeLayer(_lastDestroyTarget, 6);
                if (EditorViewPressed)
                {
                    Destroy(_lastDestroyTarget);
                    _tileDestroySound.Play();
                }
            }
            else
            {
                if (_lastDestroyTarget)
                {
                    ChangeLayer(_lastDestroyTarget, 8);
                }
            }
        }

        public void SetActiveTile(int id)
        {
            SetEditorMode(EditorMode.Place);
            if (_activeTileTransform)
                Destroy(_activeTileTransform.gameObject);
            if (id == -1) return;
            SetActiveTile(Instantiate(_tiles[id], _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), Quaternion.identity).transform);
            ChangeLayer(_activeTileTransform.gameObject, 6);
            _selectedId = id;
        }

        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            LayerMask layerMask;
            switch (_tileType)
            {
                case TileType.Obstacle:
                    layerMask = _collisionCheckObstacle;
                    break;
                case TileType.Tile:
                    layerMask = _collisionCheck;
                    break;
                case TileType.Area:
                    return;
                default:
                    return;
            }
            if (!_activeTileTransform)
                return;

            Vector3 worldCenter = _activeTileTransform.TransformPoint(_activeTileCollider.center);
            Vector3 worldHalfExtents = _activeTileTransform.TransformVector(_activeTileCollider.size * 0.45f);

            _isColliding = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, _overlapBuffer, _rotation, layerMask) > 0;
            if (_editorMode == EditorMode.Edit)
                _previewMaterial.SetColor(BaseColor, _isColliding ? _collidingColor : _editingColor);
            if (_editorMode == EditorMode.Place)
                _previewMaterial.SetColor(BaseColor, _isColliding ? _collidingColor : _notCollidingColor);
        }

        private void OnDrawGizmos()
        {
            if (!_activeTileTransform)
                return;

            Gizmos.color = Color.red;
            Vector3 worldCenter = _activeTileTransform.TransformPoint(_activeTileCollider.center);
            Vector3 worldHalfExtents = _activeTileTransform.TransformVector(_activeTileCollider.size * 0.9f);
            Gizmos.DrawWireCube(worldCenter, worldHalfExtents);
        }

        private static void ChangeLayer(GameObject gO, int layer)
        {
            gO.layer = layer;
            for(int i = 0; i < gO.transform.childCount; i++)
            {
                if (layer == 8)
                    SetLayerRecursively(gO.transform.GetChild(i), 0);
                else
                    SetLayerRecursively(gO.transform.GetChild(i), layer);
            }
        }
        private static void SetLayerRecursively(Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            for(int i = 0; i < trans.childCount; i++) { SetLayerRecursively(trans.GetChild(i), layer); }
        }

        public void SetEditorMode(int value)
        {
            if (_editorMode == (EditorMode)value) return;
            _editorMode = (EditorMode)value;
            OnEditorModeChanged?.Invoke(_editorMode);
            if (_activeTileTransform)
                Destroy(_activeTileTransform.gameObject);
            SetActiveTile(null);
            _editorUI.EditorEnabled(_editorMode != EditorMode.Off);
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }

        private void OnGameStateChanged(GameState gameState) { SetEditorMode(gameState == GameState.Editing ? EditorMode.Place : EditorMode.Off); }

        private static Transform GetTransformParent(Transform hitObject)
        {
            if (hitObject.CompareTag("Obstacle") || hitObject.CompareTag("Tile") || hitObject.CompareTag("Area"))
                return hitObject;
            return GetTransformParent(hitObject.parent);
        }

        private void SetActiveTile(Transform value)
        {
            _activeTileTransform = value;
            if (_activeTileTransform)
            {
                if (_activeTileTransform.gameObject.CompareTag("Obstacle"))
                    _tileType = TileType.Obstacle;
                if (_activeTileTransform.gameObject.CompareTag("Tile"))
                    _tileType = TileType.Tile;
                if (_activeTileTransform.gameObject.CompareTag("Area"))
                    _tileType = TileType.Area;
                _activeTileCollider = _activeTileTransform.GetComponent<BoxCollider>();
            }
            else
            {
                _activeTileCollider = null;
            }
        }

        private void SetTileToMousePos(Transform tile)
        {
            tile.position = _tileType switch
            {
                TileType.Obstacle => _cameraMain.ScreenToWorldPoint(_mousePos).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                TileType.Area => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                TileType.Tile => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10),
                _ => tile.position
            };
        }

        public bool CanStart() => _startTile && _endTile;

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }
    }

    public enum EditorMode
    {
        Place = 0,
        Destroy = 1,
        Off = 2,
        Edit = 3
    }

    public enum TileType
    {
        Tile,
        Obstacle,
        Area
    }
}
