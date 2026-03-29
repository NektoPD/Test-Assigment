using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Clicker
{
    public class PlayerPresenter : IInitializable, IDisposable
    {
        private PlayerView _playerView;
        private PlayerModel _playerModel;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public PlayerPresenter(PlayerView playerView, PlayerModel playerModel)
        {
            _playerView = playerView;
            _playerModel = playerModel;
        }

        [Inject]
        public void Initialize()
        {
            if (_playerModel == null)
                throw new ArgumentNullException(nameof(_playerModel));

            if (_playerView == null)
                throw new ArgumentNullException(nameof(_playerView));

            _playerModel.OnAutoClickPerformed.Subscribe(_ => HandleAutoClick()).AddTo(_disposables);
            _playerModel.Balance.Subscribe(_ => OnBalanceChanged()).AddTo(_disposables);
            _playerModel.Energy.Subscribe(_ => OnEnergyChanged()).AddTo(_disposables);

            _playerView.OnClickButton.Subscribe(_ => HandleButtonClick()).AddTo(_disposables);
            
            _playerView.SetEnergyAmount(_playerModel.Energy.Value, false);
            _playerView.SetBalanceAmount(_playerModel.Balance.Value, false);
            _playerView.Initialize(_playerModel.MaxEnergy, _playerModel.ClickReward);
        }

        private void HandleAutoClick()
        {
            _playerView.HandleSucessClick(_playerModel.Energy.Value, _playerModel.Balance.Value);
        }

        private void HandleButtonClick()
        {
            _playerModel.OnClickExecuted();
        }

        private void OnEnergyChanged()
        {
            _playerView.SetEnergyAmount(_playerModel.Energy.Value);
        }

        private void OnBalanceChanged()
        {
            _playerView.SetBalanceAmount(_playerModel.Balance.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}