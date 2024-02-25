using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace _game.Scripts
{
    public class TweeningComponent : MonoBehaviour
    {
        [SerializeField] private bool _startAutomatically;
        [SerializeField] private TweenType _tweenType;

        private bool _showVectorRotation;
        [SerializeField, ShowIf("_showVectorRotation")]
        private Vector3 _rotation;

        private bool _showMoveFloat;
        [SerializeField, ShowIf("_showMoveFloat")]
        private float _endValue;

        [SerializeField, MinValue(0), Space] private float _duration;
        [SerializeField] private Ease _ease;
        [SerializeField, MinValue(0)] private float _delay;

        [SerializeField, MinValue(-1)] private int _loops;
        private bool _showLoopType;
        [SerializeField, ShowIf("_showLoopType")]
        private LoopType _loopType;

        private StartTransform _startTransform;
        private Tween _tween;

        private void Awake() { _startTransform = new StartTransform(transform); }
        private void Start() { ReloadShowTriggers(); }

        private void StartTween()
        {
            switch (_tweenType)
            {
                case TweenType.DOMoveLocalX:
                    _tween = transform.DOLocalMoveX(_endValue, _duration).SetLoops(_loops, _loopType).SetEase(_ease).SetDelay(_delay);
                    break;
                case TweenType.DOMoveLocalY:
                    _tween = transform.DOLocalMoveY(_endValue, _duration).SetLoops(_loops, _loopType).SetEase(_ease).SetDelay(_delay);
                    break;
                case TweenType.DOMoveLocalZ:
                    _tween = transform.DOLocalMoveZ(_endValue, _duration).SetLoops(_loops, _loopType).SetEase(_ease).SetDelay(_delay);
                    break;
                case TweenType.DOLocalRotate:
                    _tween = transform.DOLocalRotate(_rotation, _duration).SetLoops(_loops, _loopType).SetEase(_ease).SetDelay(_delay);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy() { _tween.Kill(); }
        private void OnEnable()
        {
            if (_startAutomatically) StartTween();
        }
        private void OnDisable()
        {
            _tween.Kill();
            ResetTransform();
        }

        private void ResetTransform()
        {
            transform.SetLocalPositionAndRotation(_startTransform.Position, _startTransform.Rotation);
            transform.localScale = _startTransform.Scale;
        }

        private void OnValidate()
        {
            ReloadShowTriggers();
            if (!Application.isPlaying) return;
            _tween.Kill();
            ResetTransform();
            StartTween();
        }

        private void ReloadShowTriggers()
        {
            _showVectorRotation = _tweenType is TweenType.DOLocalRotate;
            _showMoveFloat = _tweenType is TweenType.DOMoveLocalX or TweenType.DOMoveLocalY or TweenType.DOMoveLocalZ;
            _showLoopType = _loops != 0;
        }

        private enum TweenType
        {
            DOMoveLocalX,
            DOMoveLocalY,
            DOMoveLocalZ,
            DOLocalRotate
        }

        private struct StartTransform
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public StartTransform(Transform transform)
            {
                Position = transform.localPosition;
                Rotation = transform.localRotation;
                Scale = transform.localScale;
            }
        }
    }
}
