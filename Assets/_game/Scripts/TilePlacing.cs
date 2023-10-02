using UnityEngine;

namespace _game.Scripts
{
    public class TilePlacing : MonoBehaviour
    {
        [SerializeField] private GameObject[] _tiles;
        [SerializeField] private GameObject _editorCanvas;
        private Transform _activeTile;
        private Vector3 _mousePos;
        private int _lastId;
        private bool _isColliding;
        private Quaternion _rotation;
        private EditorMode _editorMode = EditorMode.Place;
        private Collider[] _overlapBuffer = new Collider[1];


        private void Update()
        {
            _mousePos = Input.mousePosition;
            _mousePos.z = Camera.main.nearClipPlane + Camera.main.transform.position.y;

            if (_editorMode == EditorMode.Place)
                HandlePlaceMode();
            if (_editorMode == EditorMode.Destroy)
                HandleDestroyMode();
        }

        private void HandlePlaceMode()
        {
            if (!_activeTile)
                return;

            _activeTile.position = ExtensionMethods.RoundToMultiple(Camera.main.ScreenToWorldPoint(_mousePos), 10);

            if (Input.GetMouseButtonUp(0)) {

            }

            if (Input.GetKeyDown(KeyCode.R)) {
                _activeTile.transform.Rotate(new(0, 90, 0));
                _rotation = _activeTile.transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (_activeTile)
                    Destroy(_activeTile.gameObject);
                _activeTile = null;
            }
        }

        public void EditorPointerUp()
        {
            if (!_activeTile || _isColliding || _editorMode != EditorMode.Place || Input.GetMouseButtonUp(1)) return;
            ChangeLayer(_activeTile.gameObject, 0);
            if (_activeTile.TryGetComponent(out TileController tileController))
                tileController.SetActiveArrows(false);
            _activeTile = Instantiate(_tiles[_lastId], Camera.main.ViewportToWorldPoint(Input.mousePosition), _rotation).transform;
            ChangeLayer(_activeTile.gameObject, 6);
        }

        private void HandleDestroyMode()
        {
            if (Input.GetMouseButtonUp(0)) {
                Ray ray = Camera.main.ScreenPointToRay(_mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Destroy(hit.transform.gameObject);
                }
            }
        }

        public void SetActiveTile(int id)
        {
            SetEditorMode(0);
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            _activeTile = Instantiate(_tiles[id], Camera.main.ScreenToWorldPoint(_mousePos).RoundToMultiple(10), Quaternion.identity).transform;
            ChangeLayer(_activeTile.gameObject, 6);
            _lastId = id;
        }

        void FixedUpdate()
        {
            MyCollisions();
        }

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

        private void ChangeLayer(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
                t.gameObject.layer = layer;
        }

        public void SetEditorMode(int value)
        {
            _editorMode = (EditorMode)value;
            if (_activeTile)
                Destroy(_activeTile.gameObject);
            _activeTile = null;
            if (_editorMode == EditorMode.Off)
                _editorCanvas.SetActive(false);
        }

        private enum EditorMode : int
        {
            Place = 0,
            Destroy = 1,
            Off = 2
        }
    }
}
