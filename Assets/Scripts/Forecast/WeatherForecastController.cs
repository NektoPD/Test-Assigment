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
        private readonly WeatherForecastConfig _forecastConfig;
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public WeatherForecastController(
            WeatherForecastService weatherService,
            WeatherForecastView view,
            WeatherJSONParser parser,
            WeatherForecastConfig config)
        {
            _weatherService = weatherService;
            _view = view;
            _parser = parser;
            _forecastConfig = config;
        }

        public void Initialize()
        {
            _weatherService.RequestCompleted.Subscribe(OnRequestCompleted).AddTo(_disposables);
            _weatherService.RequestFailed.Subscribe(_ => OnRequestFailed()).AddTo(_disposables);

            _view.Enabled.Subscribe(_ => StartRequests()).AddTo(_disposables);
            _view.Disabled.Subscribe(_ => StopRequests()).AddTo(_disposables);
            
            StartRequests();
        }

        private void StartRequests()
        {
            _weatherService.StartAllRequests();
        }

        private void StopRequests()
        {
            _weatherService.StopAllRequests();
        }

        private void OnRequestCompleted(RequestResult result)
        {
            if (result.Success && !string.IsNullOrEmpty(result.Json))
            {
                var weatherData = _parser.ParseWeatherData(result.Json);
                _view.UpdateWeatherDisplay(weatherData);
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