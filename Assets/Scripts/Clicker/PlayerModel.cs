using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Clicker
{
    public class PlayerModel : IInitializable, IDisposable
    {
        private ClickerGameConfig _clickerGameConfig;
        private IDisposable _autoClickTimer;
        private IDisposable _autoEnergyRefill;

        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<float> _balance = new();
        private readonly ReactiveProperty<float> _energy = new();
        private readonly Subject<Unit> _autoClickPerformed = new();

        public IReadOnlyReactiveProperty<float> Balance => _balance;
        public IReadOnlyReactiveProperty<float> Energy => _energy;
        public IObservable<Unit> OnAutoClickPerformed => _autoClickPerformed;
        public float MaxEnergy => _clickerGameConfig.PlayerMaxEnergy;
        public float ClickReward => _clickerGameConfig.RewardPerClick;

        public PlayerModel(ClickerGameConfig config)
        {
            _clickerGameConfig = config;
        }

        [Inject]
        public void Initialize()
        {
            if (_clickerGameConfig == null)
                throw new ArgumentNullException(nameof(_clickerGameConfig));

            _balance.Value = 0;
            _energy.Value = _clickerGameConfig.PlayerStartEnergy;

            StartAutoClick();
            StartAutoRefill();
        }

        public void OnClickExecuted()
        {
            if (!HasEnoughEnergy())
                return;

            _balance.Value += _clickerGameConfig.RewardPerClick;
            ReduceEnergy();
        }

        public void StopAutoClicker()
        {
            _autoClickTimer?.Dispose();
            _autoClickTimer = null;
        }

        public void ResumeAutoClicker()
        {
            if (_autoClickTimer == null)
            {
                StartAutoClick();
            }
        }

        private void ReduceEnergy()
        {
            _energy.Value = Math.Clamp(_energy.Value - _clickerGameConfig.EnergyCostPerClick, 0,
                _clickerGameConfig.PlayerMaxEnergy);
        }

        private void StartAutoClick()
        {
            _autoClickTimer?.Dispose();

            _autoClickTimer = Observable
                .Interval(TimeSpan.FromSeconds(_clickerGameConfig.AutoClickDelaySeconds))
                .Subscribe(_ => ExecuteAutoClick())
                .AddTo(_disposables);
        }

        private void StartAutoRefill()
        {
            _autoEnergyRefill?.Dispose();

            _autoEnergyRefill = Observable
                .Interval(TimeSpan.FromSeconds(_clickerGameConfig.EnergyRefillDelaySeconds))
                .Subscribe(_ => ExecuteEnergyAutoRefill())
                .AddTo(_disposables);
        }

        private void ExecuteAutoClick()
        {
            if (!HasEnoughEnergy())
                return;

            _balance.Value += _clickerGameConfig.RewardPerClick;
            ReduceEnergy();

            _autoClickPerformed.OnNext(Unit.Default);
        }

        private void ExecuteEnergyAutoRefill()
        {
            if (_energy.Value >= _clickerGameConfig.PlayerMaxEnergy)
                return;

            _energy.Value = Math.Clamp(_energy.Value + _clickerGameConfig.EnergyRefillAmount, 0,
                _clickerGameConfig.PlayerMaxEnergy);
        }

        private bool HasEnoughEnergy()
        {
            return _energy.Value >= _clickerGameConfig.EnergyCostPerClick;
        }

        public void Dispose()
        {
            _autoClickTimer?.Dispose();
            _disposables.Dispose();
            _balance.Dispose();
            _energy.Dispose();
            _autoClickPerformed.Dispose();
        }
    }
}