using System;
using _game.Scripts.UIScripts;
using UnityEngine;
using FMOD;
using FMODUnity;
using JetBrains.Annotations;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour
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
        private int _activeTileDefaultLayer;
        private Vector3 _mousePos;
        [SerializeField] private int _selectedId;
        [SerializeField] private StudioEventEmitter _tilePlaceSound, _tileDestroySound;
        private bool _isColliding;
        private Layer _layer;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[1];
        private Camera _cameraMain;
        [SerializeField] private GameObject _startTile, _endTile;
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
                        ChangeCollisionLayer(_activeTileTransform.gameObject, _activeTileDefaultLayer);
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
                    if (_lastEditTarget && _lastEditTarget != hitGameObject)
                        ChangeCollisionLayer(_lastEditTarget, _activeTileDefaultLayer, true);
                    _lastEditTarget = hitGameObject;
                    ChangeCollisionLayer(_lastEditTarget, (int)Layer.NoCollision, true);
                    if (EditorViewPressed)
                    {
                        SetActiveTile(hitGameObject);
                        ChangeCollisionLayer(hitGameObject, (int)Layer.NoCollision);
                        if (_activeTileTransform.TryGetComponent(out TileController tileController))
                            tileController.SetActiveArrows(true);
                        _tileDestroySound.Play();
                    }
                }
                else
                {
                    if (_lastEditTarget)
                        ChangeCollisionLayer(_lastEditTarget, _activeTileDefaultLayer);
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
            ChangeCollisionLayer(_activeTileTransform.gameObject, _activeTileDefaultLayer);
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
            ChangeCollisionLayer(_activeTileTransform.gameObject, (int)Layer.NoCollision);
            _tilePlaceSound.Play();
        }

        private void HandleDestroyMode()
        {
            Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, _editorRaycast))
            {
                SetHighlightMaterialColor(_collidingColor);
                GameObject hitObject = hit.transform.gameObject;
                if (_lastDestroyTarget && _lastDestroyTarget != hitObject.gameObject)
                    ChangeCollisionLayer(_lastDestroyTarget, _activeTileDefaultLayer, true);
                _lastDestroyTarget = hitObject.gameObject;
                ChangeCollisionLayer(_lastDestroyTarget, (int)Layer.NoCollision, true);
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
                    ChangeCollisionLayer(_lastDestroyTarget, _activeTileDefaultLayer);
                }
            }
        }

        public void SetActiveTile(int id)
        {
            SetEditorMode(EditorMode.Place);
            if (_activeTileTransform)
                Destroy(_activeTileTransform.gameObject);
            if (id == -1) return;
            SetActiveTile(Instantiate(_tiles[id], _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), _rotation).transform);
            ChangeCollisionLayer(_activeTileTransform.gameObject, (int)Layer.NoCollision);
            _selectedId = id;
        }

        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            if (!_activeTileTransform)
                return;

            Vector3 worldCenter = _activeTileTransform.TransformPoint(_activeTileCollider.center);
            Vector3 worldHalfExtents = _activeTileTransform.TransformVector(_activeTileCollider.size * 0.45f);
            LayerMask layerMask = GetActiveTileCollisionCheck(_layer);

            // _test.localPosition = worldCenter;
            // _test.localScale = worldHalfExtents * 2;
            // _test.localRotation = _rotation;

            _isColliding = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, _overlapBuffer, _rotation, layerMask, QueryTriggerInteraction.Ignore) > 0;
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
                Layer.Road => _collisionCheckRoad,
                Layer.Area => _collisionCheckArea,
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

        private static void ChangeCollisionLayer(GameObject gO, int layer, bool renderOnly = false)
        {
            if (!renderOnly)
                gO.layer = layer;
            for(int i = 0; i < gO.transform.childCount; i++)
            {
                int childLayer = gO.transform.GetChild(i).gameObject.layer;
                if (IsDefaultCollision(layer))
                {
                    if (childLayer == (int)Layer.NoCollision && !renderOnly)
                        SetLayerRecursively(gO.transform.GetChild(i), (int)Layer.Default);
                    if (childLayer == (int)Layer.PickedUpRender)
                        SetLayerRecursively(gO.transform.GetChild(i), (int)Layer.NormalRender);
                }
                else
                {
                    if (childLayer == (int)Layer.Default && !renderOnly)
                        SetLayerRecursively(gO.transform.GetChild(i), (int)Layer.NoCollision);
                    if (childLayer == (int)Layer.NormalRender)
                        SetLayerRecursively(gO.transform.GetChild(i), (int)Layer.PickedUpRender);
                }
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
            SetActiveTile((Transform)null);
            _editorUI.EditorEnabled(_editorMode != EditorMode.Off);
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }

        private void OnGameStateChanged(GameState gameState) { SetEditorMode(gameState == GameState.Editing ? EditorMode.Place : EditorMode.Off); }

        private void SetActiveTile(GameObject value) { SetActiveTile(value.transform); }
        private void SetActiveTile(Transform value)
        {
            _activeTileTransform = value;
            if (_activeTileTransform)
            {
                if (_activeTileTransform.gameObject.CompareTag("Obstacle"))
                    _layer = Layer.Object;
                if (_activeTileTransform.gameObject.CompareTag("Tile"))
                    _layer = Layer.Road;
                if (_activeTileTransform.gameObject.CompareTag("Area"))
                    _layer = Layer.Area;
                _activeTileCollider = _activeTileTransform.GetComponent<BoxCollider>();
                _activeTileDefaultLayer = _activeTileTransform.gameObject.layer;
            }
            else
            {
                _activeTileCollider = null;
            }
        }

        private void SetHighlightMaterialColor(Color color)
        {
            _outlineMat.SetColor(BaseColor, color);
            _tintMat.SetColor(BaseColor, color);
        }

        private void SetTileToMousePos(Transform tile)
        {
            tile.position = _layer switch
            {
                Layer.Object => _cameraMain.ScreenToWorldPoint(_mousePos).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.Area => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10).MultiplyBy(new Vector3(1, 0, 1)) + Vector3.up,
                Layer.Road => _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10),
                _ => tile.position
            };
        }

        private static bool IsDefaultCollision(int layer) { return layer is (int)Layer.Road or (int)Layer.Object or (int)Layer.Area; }

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

    public enum Layer
    {
        Default = 0,
        Car = 3,
        NoCollision = 6,
        Road = 7,
        Object = 8,
        Area = 9,
        NormalRender = 11,
        PickedUpRender = 12
    }
}
