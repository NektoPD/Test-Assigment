using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Clicker
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private AnimatedButton _clickButton;
        [SerializeField] private AnimatedText _animatedBalanceText;
        [SerializeField] private AnimatedText _animatedEnergyText;
        [SerializeField] private AudioController _audioController;
        [SerializeField] private Transform _floatingTextSpawnPoint;
        [SerializeField] private Transform _floatingTextTarget;

        private FloatingText.Factory _floatingTextFactory;
        private ClickParticle.Factory _clickParticleFactory;
        
        private float _maxEnergyValue;
        private float _rewardValue;
        private readonly Subject<Unit> _onClickButton = new Subject<Unit>();
        public IObservable<Unit> OnClickButton => _onClickButton;
        
        [Inject]
        public void Construct(FloatingText.Factory floatingTextFactory, ClickParticle.Factory clickParticleFactory)
        {
            _floatingTextFactory = floatingTextFactory;
            _clickParticleFactory = clickParticleFactory;
        }
        
        private void OnEnable()
        {
            _clickButton.OnClicked += OnClickPerfomed;
        }

        private void OnDisable()
        {
            _clickButton.OnClicked -= OnClickPerfomed;
        }

        public void SetEnergyAmount(float energyCount, bool animated = true)
        {
            _animatedEnergyText.SetValue(energyCount, animated);
        }

        public void SetBalanceAmount(float balanceCount, bool animated = true)
        {
            _animatedBalanceText.SetValue(balanceCount, animated);
        }

        public void Initialize(float maxEnergy, float rewardValue)
        {
            _maxEnergyValue = maxEnergy;
            _animatedEnergyText.SetMaxValue(_maxEnergyValue);

            _rewardValue = rewardValue;
        }

        public void HandleSucessClick(float enegryCount, float balance)
        {
            SetEnergyAmount(enegryCount);
            SetBalanceAmount(balance);

            EnableClickVFX();
        }

        private void OnClickPerfomed()
        {
            _onClickButton.OnNext(Unit.Default);
            
            EnableClickVFX();
        }

        private void EnableClickVFX()
        {
            SpawnClickParticle();
            SpawnFloatingText(_rewardValue);
            _audioController.PlayClickSound();
            _clickButton.PlayClickAnimation();
        }
        
        private void SpawnClickParticle()
        {
            if (_clickParticleFactory != null)
            {
                Vector3 particlePosition = _clickButton.transform.position;
                _clickParticleFactory.Create(particlePosition);
            }
        }
        
        private void SpawnFloatingText(float reward)
        {
            if (_floatingTextFactory != null)
            {
                Vector3 startPosition = _floatingTextSpawnPoint != null 
                    ? _floatingTextSpawnPoint.position 
                    : _clickButton.transform.position;
                
                Vector3 targetPosition = _floatingTextTarget != null 
                    ? _floatingTextTarget.position 
                    : _animatedBalanceText.transform.position;
                
                var floatingText = _floatingTextFactory.Create(reward, targetPosition);
                floatingText.transform.position = startPosition;
            }
        }
        
        private void OnDestroy()
        {
            _onClickButton.Dispose();
        }
    }
}