using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BreedFacts
{
    public class BreedController : IInitializable, IDisposable
    {
        private readonly BreedScreenView _view;
        private readonly BreedService _service;

        private CancellationTokenSource _cts;
        private int _currentIndex = -1;

        public BreedController(BreedScreenView view, BreedService service)
        {
            _view = view;
            _service = service;
        }

        public void Initialize()
        {
            LoadList().Forget();
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
                    var breed = breeds[i];

                    button.SetData(i + 1, breed.Name);

                    button.Button.onClick.RemoveAllListeners();
                    button.Button.onClick.AddListener(() =>
                    {
                        OnClick(index).Forget();
                    });
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private async UniTaskVoid OnClick(int index)
        {
            if (_currentIndex == index)
                return;

            _service.Cancel();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (_currentIndex >= 0)
                _view.Buttons[_currentIndex].ShowLoader(false);

            _currentIndex = index;

            var button = _view.Buttons[index];
            button.ShowLoader(true);

            var breed = _service.Cached[index];
            var result = await _service.LoadBreed(breed.Id, _cts.Token);

            button.ShowLoader(false);

            if (result != null)
                _view.Popup.Show(result);
        }

        public void Dispose()
        {
            _cts?.Cancel();
        }
    }
}