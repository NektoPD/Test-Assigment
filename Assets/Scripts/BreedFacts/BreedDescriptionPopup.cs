using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace BreedFacts
{
    public class BreedDescriptionPopup : MonoBehaviour
    {
        [SerializeField] private RectTransform _frame;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _okButton;

        private readonly Subject<Unit> _onClosed = new Subject<Unit>();

        public IObservable<Unit> OnClosed => _onClosed;

        private void Awake()
        {
            _okButton.onClick.AddListener(Close);
        }

        public void Show(BreedData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            gameObject.SetActive(true);

            _name.text = data.Name;
            _description.text = data.Description;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _onClosed.OnNext(Unit.Default);
        }

        private void OnDisable()
        {
            _onClosed.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onClosed?.Dispose();
        }
    }
}