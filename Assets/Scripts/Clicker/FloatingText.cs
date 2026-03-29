using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Clicker
{
    public class FloatingText : MonoBehaviour, IPoolable<float, Vector3, IMemoryPool>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;
        
        [SerializeField] private float _flyDuration = 0.5f;
        [SerializeField] private float _flyHeight = 1f;
        
        [SerializeField] private float _appearDuration = 0.2f;
        [SerializeField] private float _appearScale = 1f;
        [SerializeField] private float _appearAlpha = 1f;
        [SerializeField] private Ease _appearEase = Ease.OutBack;
        
        [SerializeField] private float _disappearDuration = 0.2f;
        [SerializeField] private float _disappearScale = 0f;
        [SerializeField] private float _disappearAlpha = 0f;
        [SerializeField] private Ease _disappearEase = Ease.InBack;
        
        [SerializeField] private PathType _flightPathType = PathType.CatmullRom;
        [SerializeField] private Ease _flightEase = Ease.InQuad;
        
        [SerializeField] private float _startScale = 0f;
        [SerializeField] private float _startAlpha = 0f;
        
        private IMemoryPool _pool;
        private Vector3 _targetPosition;
        private Sequence _animationSequence;
        
        private void Awake()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }
        
        public void OnSpawned(float value, Vector3 targetPosition, IMemoryPool pool)
        {
            gameObject.SetActive(true);
            _targetPosition = targetPosition;
            _pool = pool;
            
            _text.text = $"+{value:F0}";
            _text.color = Color.yellowNice;
            transform.localScale = Vector3.one * _startScale;
            _canvasGroup.alpha = _startAlpha;
            
            Animate();
        }
        
        public void OnDespawned()
        {
            _animationSequence?.Kill();
            _animationSequence = null;
            _pool = null;
            gameObject.SetActive(false);
        }
        
        private void Animate()
        {
            _animationSequence = DOTween.Sequence();
            
            Vector3 startPosition = transform.position;
            Vector3 middlePosition = startPosition + Vector3.up * _flyHeight;
            
            _animationSequence.Append(transform.DOScale(_appearScale, _appearDuration).SetEase(_appearEase));
            _animationSequence.Join(_canvasGroup.DOFade(_appearAlpha, _appearDuration));
            _animationSequence.Append(transform.DOPath(new Vector3[] { middlePosition, _targetPosition }, _flyDuration, _flightPathType).SetEase(_flightEase));
            _animationSequence.Append(transform.DOScale(_disappearScale, _disappearDuration).SetEase(_disappearEase));
            _animationSequence.Join(_canvasGroup.DOFade(_disappearAlpha, _disappearDuration));
            _animationSequence.OnComplete(() => _pool?.Despawn(this));
        }

        public class Factory : PlaceholderFactory<float, Vector3, FloatingText> { }
    }
}