using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Clicker
{
    public class AnimatedText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private float _scaleMultiplier = 1.2f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;
        [SerializeField] private Ease _numberEase = Ease.OutQuad;
        [SerializeField] private bool _hasMaxValue;

        private Sequence _currentScaleTween;
        private Tweener _currentNumberTween;
        private float _currentValue;
        private float _displayValue;
        private float _maxValue;

        public void SetMaxValue(float maxValue)
        {
            _maxValue = maxValue;
        }
        
        public void SetValue(float newValue, bool animate = true)
        {
            if (Mathf.Approximately(_currentValue, newValue) && animate)
            {
                AnimateText();
                return;
            }

            float oldValue = _currentValue;
            _currentValue = newValue;

            if (animate)
            {
                AnimateNumber(oldValue, newValue);
                AnimateText();
            }
            else
            {
                _displayValue = newValue;
                _text.text = _hasMaxValue ? $"{newValue:F0}/{_maxValue:F0}" : newValue.ToString();
            }
        }

        private void AnimateNumber(float from, float to)
        {
            _currentNumberTween?.Kill();

            _currentNumberTween = DOTween.To(
                () => from,
                x =>
                {
                    _displayValue = Mathf.RoundToInt(x);
                    _text.text = _hasMaxValue ? $"{_displayValue:F0}/{_maxValue:F0}" : _displayValue.ToString();
                },
                to,
                _animationDuration
            ).SetEase(_numberEase);
        }

        private void AnimateText()
        {
            _currentScaleTween?.Kill();

            Sequence sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(_scaleMultiplier, _animationDuration / 2).SetEase(_scaleEase));
            sequence.Append(transform.DOScale(1f, _animationDuration / 2).SetEase(Ease.OutBack));

            _currentScaleTween = sequence;
        }
    }
}