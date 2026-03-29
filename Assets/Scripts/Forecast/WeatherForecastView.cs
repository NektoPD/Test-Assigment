using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Forecast
{
    public class WeatherForecastView : MonoBehaviour
    {
        [SerializeField] private string _errorMessage;
        [SerializeField] private Image _weatherImage;
        [SerializeField] private TMP_Text _weatherText;

        [SerializeField] private Sprite _errorSprite;

        [SerializeField] private GameObject _loadingObject;
        [SerializeField] private GameObject _contentObject;
        
        public IObservable<Unit> Disabled => _disabled;
        private readonly Subject<Unit> _disabled = new Subject<Unit>();
        
        public IObservable<Unit> Enabled => _enabled;
        private readonly Subject<Unit> _enabled = new Subject<Unit>();

        private void OnEnable()
        {
            _enabled.OnNext(Unit.Default);
        }

        private void OnDisable()
        {
            _disabled.OnNext(Unit.Default);
        }

        public async void UpdateWeatherDisplay(WeatherData data)
        {
            if (data == null)
            {
                ShowError();
                return;
            }

            try
            {
                ShowLoading();
                
                _weatherText.text = $"{data.Name} - {data.Temperature}{data.TemperatureUnit}";

                if (!string.IsNullOrEmpty(data.Icon))
                {
                    await LoadIcon(data.Icon);
                }
            }
            catch (Exception)
            {
                ShowError();
            }
            finally
            {
                HideLoading();
            }
        }
        
        public void ShowError()
        {
            HideLoading();
            _weatherText.text = _errorMessage;
            _weatherImage.sprite = _errorSprite;
        }

        private void ShowLoading()
        {
            _loadingObject.SetActive(true);
            _contentObject.SetActive(false);
        }

        private void HideLoading()
        {
            _loadingObject.SetActive(false);
            _contentObject.SetActive(true);
        }
        
        private async UniTask LoadIcon(string iconUrl)
        {
            try
            {
                using (var webRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(iconUrl))
                {
                    await webRequest.SendWebRequest();
                    
                    if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(webRequest);
                        if (texture != null)
                        {
                            var sprite = Sprite.Create(
                                texture, 
                                new Rect(0, 0, texture.width, texture.height), 
                                new Vector2(0.5f, 0.5f)
                            );
                            _weatherImage.sprite = sprite;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _weatherImage.sprite = _errorSprite;
                throw new ArgumentException(e.Message);
            }
        }
    }
}