using BreedFacts;
using Breeds;
using Core;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class BreedInstaller : MonoInstaller
    {
        [SerializeField] private BreedRequestConfig _config;
        [SerializeField] private BreedScreenView _view;

        public override void InstallBindings()
        {
            Container.Bind<BreedRequestConfig>()
                .FromInstance(_config)
                .AsSingle();

            Container.Bind<BreedScreenView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<JsonParser>()
                .AsSingle()
                .IfNotBound();

            Container.Bind<WebRequestService>()
                .AsSingle()
                .IfNotBound();

            Container.Bind<BreedService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BreedController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
