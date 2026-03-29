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
        
        private IMemoryPool _pool;
        private Vector3 _targetPosition;
        private Sequence _animationSequence;
        
        private void Awake()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();
            
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            gameObject.SetActive(false);
        }
        
        public void OnSpawned(float value, Vector3 targetPosition, IMemoryPool pool)
        {
            gameObject.SetActive(true);
            
            _targetPosition = targetPosition;
            _pool = pool;
            
            _text.text = $"+{value:F0}";
            _text.color = Color.white;

            transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition3D = Vector3.zero;
            }
            
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
            
            _animationSequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            _animationSequence.Join(_canvasGroup.DOFade(1f, 0.15f));
            
            _animationSequence.Append(transform.DOPath(new Vector3[] { middlePosition, _targetPosition }, 
                _flyDuration, PathType.CatmullRom).SetEase(Ease.InQuad));
            
            _animationSequence.Append(transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
            _animationSequence.Join(_canvasGroup.DOFade(0f, 0.2f));
            
            _animationSequence.OnComplete(() =>
            {
                if (_pool != null)
                {
                    _pool.Despawn(this);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            });
        }

        public void StopAndReturn()
        {
            _animationSequence?.Kill();
            if (_pool != null)
            {
                _pool.Despawn(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        public class Factory : PlaceholderFactory<float, Vector3, FloatingText>
        {
        }
    }
}