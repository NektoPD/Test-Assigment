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
            // Конфиг
            Container.Bind<WeatherForecastConfig>()
                .FromInstance(_config)
                .AsSingle();

            // View
            Container.Bind<WeatherForecastView>()
                .FromInstance(_view)
                .AsSingle();

            // Парсер
            Container.Bind<WeatherJSONParser>()
                .AsSingle();

            // Сервис
            Container.Bind<WeatherForecastService>()
                .AsSingle()
                .OnInstantiated<WeatherForecastService>((ctx, instance) => 
                {
                    Debug.Log("[WeatherForecastInstaller] WeatherForecastService создан");
                })
                .NonLazy(); // Создаем сразу при старте

            // Контроллер
            Container.BindInterfacesAndSelfTo<WeatherForecastController>()
                .AsSingle()
                .NonLazy();
        }
    }
}