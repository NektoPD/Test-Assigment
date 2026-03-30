using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace BreedFacts
{
    public class BreedController : IInitializable, IDisposable
    {
        private readonly BreedScreenView _view;
        private readonly BreedService _service;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private CancellationTokenSource _cts;
        private int _currentIndex = -1;

        public BreedController(BreedScreenView view, BreedService service)
        {
            _view = view;
            _service = service;
        }

        public void Initialize()
        {
            _view.OnScreenOpened.Subscribe(_ => OnScreenOpened()).AddTo(_disposables);
            _view.OnScreenClosed.Subscribe(_ => OnScreenClosed()).AddTo(_disposables);
        }

        private void OnScreenOpened()
        {
            _view.ResetButtons();
            LoadList().Forget();
        }

        private void OnScreenClosed()
        {
            _cts?.Cancel();
            _service.Cancel();
            _currentIndex = -1;
        }

        private async UniTaskVoid LoadList()
        {
            var breeds = await _service.LoadBreeds();

            for (int i = 0; i < _view.Buttons.Length; i++)
            {
                var button = _view.Buttons[i];

                if (i < breeds.Count)
                {
                    int index = i;
                    button.SetData(i + 1, breeds[i].Name);

                    button.OnClick.Subscribe(_ =>
                    {
                        OnButtonClick(index).Forget();
                    }).AddTo(_disposables);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private async UniTaskVoid OnButtonClick(int index)
        {
            if (_currentIndex == index)
                return;

            _service.Cancel();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (_currentIndex >= 0 && _currentIndex < _view.Buttons.Length)
                _view.Buttons[_currentIndex].ShowLoader(false);

            _currentIndex = index;

            var button = _view.Buttons[index];
            button.ShowLoader(true);

            var breed = _service.Cached[index];
            var result = await _service.LoadBreed(breed.Id, _cts.Token);

            button.ShowLoader(false);
            _currentIndex = -1;

            if (result != null)
                _view.Popup.Show(result);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _disposables.Dispose();
        }
    }
}