using Forecast;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class WeatherForecastInstaller : MonoInstaller
    {
        [SerializeField] private WeatherForecastConfig _config;
        [SerializeField] private WeatherForecastView _view;

        public override void InstallBindings()
        {
            Container.Bind<WeatherForecastConfig>()
                .FromInstance(_config)
                .AsSingle();

            Container.Bind<WeatherForecastView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<WeatherJSONParser>()
                .AsSingle();

            Container.Bind<WeatherForecastService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<WeatherForecastController>()
                .AsSingle()
                .NonLazy();
        }
    }
}