using System;
using System.Collections.Generic;
using _game.Scripts.UIScripts;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour
    {
        [SerializeField] private GameObject[] _tiles;
        [SerializeField] private EditorUIController _editorUI;
        [SerializeField] private LayerMask _collisionCheck;
        [ColorUsage(true, true), SerializeField]
        private Color _collidingColor, _notCollidingColor;
        [SerializeField] private Material _previewMaterial;
        private Transform _activeTile;
        private Vector3 _mousePos;
        [SerializeField] private int _selectedId;
        private bool _isColliding;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private readonly Collider[] _overlapBuffer = new Collider[1];
        private Camera _cameraMain;
        private GameObject _startTile, _endTile;
        private GameObject _lastDestroyTarget;

        private void Awake() { _cameraMain = Camera.main; }

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
            if (!_activeTile || _editorMode != EditorMode.Place || !Input.GetMouseButtonUp(0)) return;
            if (_isColliding)
            {
                _editorUI.DisplayWarning(0);
                return;
            }
            ChangeLayer(_activeTile.gameObject, 8);
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
            // if (Input.GetMouseButtonUp(0))
            // {
            //     Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
            //     if (Physics.Raycast(ray, out RaycastHit hit))
            //     {
            //         Destroy(hit.transform.gameObject);
            //     }
            // }
            Ray ray = _cameraMain.ScreenPointToRay(_mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _previewMaterial.SetColor("_baseColor", _collidingColor);
                if (_lastDestroyTarget && _lastDestroyTarget != hit.transform.gameObject)
                    ChangeLayer(_lastDestroyTarget, 8);
                _lastDestroyTarget = hit.transform.gameObject;
                ChangeLayer(_lastDestroyTarget, 6);
                if (Input.GetMouseButtonUp(0))
                {
                    Destroy(_lastDestroyTarget);
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
            SetEditorMode(0);
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            if (id == -1) return;
            _activeTile = Instantiate(_tiles[id], _cameraMain.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), Quaternion.identity).transform;
            ChangeLayer(_activeTile.gameObject, 6);
            _selectedId = id;
        }

        private void FixedUpdate() { MyCollisions(); }

        private void MyCollisions()
        {
            if (_activeTile)
            {
                _isColliding = Physics.OverlapBoxNonAlloc(_activeTile.GetChild(0).position, _activeTile.GetChild(0).localScale * 0.45f, _overlapBuffer, _rotation, _collisionCheck) > 0;
                _previewMaterial.SetColor("_baseColor", _isColliding ? _collidingColor : _notCollidingColor);
            }
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
            _editorMode = (EditorMode)value;
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            _activeTile = null;
            _editorUI.EditorEnabled(_editorMode != EditorMode.Off);
        }
        private void SetEditorMode(EditorMode mode) { SetEditorMode((int)mode); }
        public void SetEditorMode(bool value) { SetEditorMode(value ? 1 : 0); }

        private enum EditorMode
        {
            Place = 0,
            Destroy = 1,
            Off = 2
        }


        private void OnGameStateChanged(GameState gameState) { SetEditorMode(gameState == GameState.Editing ? EditorMode.Place : EditorMode.Off); }

        public void OnPressPlay()
        {
            if (_startTile && _endTile)
                GameManager.GameState = 0;
            else
                _editorUI.DisplayWarning(1);
        }

        private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }
    }
}
