using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Forecast
{
    public class WeatherForecastController : IInitializable, IDisposable
    {
        private readonly WeatherForecastService _weatherService;
        private readonly WeatherForecastView _view;
        private readonly WeatherJSONParser _parser;
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public WeatherForecastController(
            WeatherForecastService weatherService,
            WeatherForecastView view,
            WeatherJSONParser parser)
        {
            _weatherService = weatherService;
            _view = view;
            _parser = parser;
        }

        public void Initialize()
        {
            Debug.Log("[WeatherForecastController] Инициализация контроллера");

            _weatherService.RequestCompleted.Subscribe(OnRequestCompleted).AddTo(_disposables);
            _weatherService.RequestFailed.Subscribe(_ => OnRequestFailed()).AddTo(_disposables);

            _view.Enabled.Subscribe(_ => StartRequests()).AddTo(_disposables);
            _view.Disabled.Subscribe(_ => StopRequests()).AddTo(_disposables);
        }

        private void StartRequests()
        {
            _weatherService.StartProcessing();
        }

        private void StopRequests()
        {
            _weatherService.Stop();
        }

        private void OnRequestCompleted(RequestResult result)
        {
            if (result.Success && !string.IsNullOrEmpty(result.Json))
            {
                var weatherData = _parser.ParseWeatherData(result.Json);
                
                if (weatherData != null)
                {
                    _view.UpdateWeatherDisplay(weatherData);
                }
                else
                {
                    _view.ShowError();
                }
            }
            else
            {
                _view.ShowError();
            } 
        }

        private void OnRequestFailed()
        {
            _view.ShowError();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}