using System;
using UnityEngine;
using UniRx;

namespace BreedFacts
{
    public class BreedScreenView : MonoBehaviour
    {
        [SerializeField] private BreedButton[] _breedButtons;
        [SerializeField] private BreedDescriptionPopup _popup;

        private readonly Subject<Unit> _onEnable = new Subject<Unit>();
        private readonly Subject<Unit> _onDisable = new Subject<Unit>();

        public IObservable<Unit> OnScreenOpened => _onEnable;
        public IObservable<Unit> OnScreenClosed => _onDisable;

        public BreedButton[] Buttons => _breedButtons;
        public BreedDescriptionPopup Popup => _popup;

        private void OnEnable()
        {
            _onEnable.OnNext(Unit.Default);
        }

        private void OnDisable()
        {
            _onDisable.OnNext(Unit.Default);
        }

        public void ShowLoadingAll(bool value)
        {
            foreach (var button in _breedButtons)
            {
                button.ShowLoader(value);
                button.SetInteractable(!value);
            }
        }

        public void ResetButtons()
        {
            foreach (var button in _breedButtons)
            {
                button.gameObject.SetActive(false);
                button.ShowLoader(false);
            }
        }

        private void OnDestroy()
        {
            _onEnable?.Dispose();
            _onDisable?.Dispose();
        }
    }
}