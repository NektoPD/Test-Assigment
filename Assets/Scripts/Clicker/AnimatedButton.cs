using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Clicker
{
    public class AnimatedButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;

        [SerializeField] private float _pressScale = 0.9f;
        [SerializeField] private float _targetScale = 1.0f;
        [SerializeField] private float _animationDuration = 0.1f;
        [SerializeField] private Ease _easeType = Ease.OutBack;

        [SerializeField] private Sprite[] _clickSprites;

        private Tweener _scaleTween;
        private Sprite _originalSprite;

        public event Action OnClicked;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_buttonImage == null && _button != null)
                _buttonImage = _button.GetComponent<Image>();

            if (_buttonImage != null)
                _originalSprite = _buttonImage.sprite;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
            _scaleTween?.Kill();
        }

        private void HandleClick()
        {
            PlayClickAnimation();
            OnClicked?.Invoke();
        }

        public void PlayClickAnimation()
        {
            if (!_button.interactable) return;

            _scaleTween?.Kill();

            _scaleTween = transform
                .DOScale(_pressScale, _animationDuration)
                .SetEase(_easeType)
                .OnComplete(() =>
                {
                    transform
                        .DOScale(_targetScale, _animationDuration)
                        .SetEase(_easeType);
                });

            ChangeSprite();
        }

        private void ChangeSprite()
        {
            if (_clickSprites == null || _clickSprites.Length == 0) return;

            int randomIndex = Random.Range(0, _clickSprites.Length);
            Sprite newSprite = _clickSprites[randomIndex];
            _buttonImage.sprite = newSprite;
        }
    }
}