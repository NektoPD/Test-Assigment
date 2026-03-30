using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace BreedFacts
{
    public class BreedButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _id;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject _loader;
        [SerializeField] private Button _button;

        private readonly Subject<Unit> _onClick = new Subject<Unit>();

        public IObservable<Unit> OnClick => _onClick;

        public void SetData(int index, string name)
        {
            _id.text = index.ToString();
            _name.text = name;

            gameObject.SetActive(true);
            ShowLoader(false);
        }

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                _onClick.OnNext(Unit.Default);
            });
        }

        public void ShowLoader(bool value)
        {
            if (_loader != null)
                _loader.SetActive(value);
        }

        public void SetInteractable(bool value)
        {
            if (_button != null)
                _button.interactable = value;
        }

        private void OnDestroy()
        {
            _onClick?.Dispose();
        }
    }
}