using System;
using UnityEngine;
using DG.Tweening;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour
    {
        [SerializeField] private GameObject[] _tiles;
        [SerializeField] private Canvas _editorCanvas;
        [SerializeField] private Transform _hideArrow;
        private Transform _activeTile;
        private Vector3 _mousePos;
        [SerializeField] private int _selectedId;
        private bool _isColliding;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[1];
        private RectTransform _rectTransform;
        private Camera _cameraMain;
        private GameObject _startTile, _endTile;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _cameraMain = Camera.main;
        }

        private void Update()
        {
            _mousePos = Input.mousePosition;
            _mousePos.z = _cameraMain.nearClipPlane + _cameraMain.transform.position.y;

            if (_editorMode == EditorMode.Place)
                HandlePlaceMode();
            if (_editorMode == EditorMode.Destroy)
                HandleDestroyMode();
        }

        private void HandlePlaceMode()
        {
            if (!_activeTile)
                return;

            _activeTile.position = _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10);

            if (Input.GetKeyDown(KeyCode.R))
            {
                _activeTile.transform.Rotate(new(0, 90, 0));
                _rotation = _activeTile.transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(_activeTile.gameObject);
                _activeTile = null;
            }
        }

        public void EditorPointerUp()
        {
            if (!_activeTile || _isColliding || _editorMode != EditorMode.Place || !Input.GetMouseButtonUp(0)) return;
            ChangeLayer(_activeTile.gameObject, 0);
            if (_activeTile.TryGetComponent(out TileController tileController))
                tileController.SetActiveArrows(false);
            if (_selectedId == 4) //4 is StartTileId
            {
                DestroyImmediate(_startTile);
                _startTile = _activeTile.gameObject;
            }
            if (_selectedId == 5) //5 is EndTileId
            {
                DestroyImmediate(_endTile);
                _endTile = _activeTile.gameObject;
            }
            _activeTile = Instantiate(_tiles[_selectedId], _cameraMain.ViewportToWorldPoint(Input.mousePosition), _rotation).transform;
            ChangeLayer(_activeTile.gameObject, 6);
        }

        private void HandleDestroyMode()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Destroy(hit.transform.gameObject);
                }
            }
        }

        public void SetActiveTile(int id)
        {
            SetEditorMode(0);
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            _activeTile = Instantiate(_tiles[id], _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), Quaternion.identity).transform;
            ChangeLayer(_activeTile.gameObject, 6);
            _selectedId = id;
        }

        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            if (_activeTile)
                _isColliding = Physics.OverlapBoxNonAlloc(_activeTile.GetChild(0).position, _activeTile.GetChild(0).localScale * 0.45f, _overlapBuffer, _rotation, ~(1 << 6)) > 0;
        }

        void OnDrawGizmos()
        {
            if (!_activeTile)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_activeTile.GetChild(0).position, _activeTile.GetChild(0).localScale * 0.9f);
        }

        private static void ChangeLayer(GameObject gO, int layer)
        {
            gO.layer = layer;
            foreach (Transform t in gO.transform)
                t.gameObject.layer = layer;
        }

        public void SetEditorMode(int value)
        {
            _editorMode = (EditorMode)value;
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            _activeTile = null;
            if (_editorMode == EditorMode.Off)
                _editorCanvas.enabled = false;
            else
                _editorCanvas.enabled = true;
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }

        private enum EditorMode
        {
            Place = 0,
            Destroy = 1,
            Off = 2
        }

        public void HideTileMenu(bool value)
        {
            if (value)
            {
                _rectTransform.DOAnchorPosY(-245, 0.5f);
                _hideArrow.DORotate(Vector3.zero, 0.5f);
            }
            else
            {
                _rectTransform.DOAnchorPosY(0, 0.5f);
                _hideArrow.DORotate(new(0, 0, -180), 0.5f);
            }
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Editing)
            {
                SetEditorMode(EditorMode.Place);
            }
            else
            {
                SetEditorMode(EditorMode.Off);
            }
        }

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }
    }
}
