using Clicker;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ClickerGameInstaller : MonoInstaller
    {
        [SerializeField] private ClickerGameConfig _clickerGameConfig;
        [SerializeField] private PlayerView _playerView;

        [SerializeField] private FloatingText _floatingTextPrefab;
        [SerializeField] private ClickParticle _clickParticlePrefab;
        [SerializeField] private int _poolInitialSize = 10;
        [SerializeField] private Transform _poolParent;

        public override void InstallBindings()
        {
            Container.BindInstance(_clickerGameConfig).AsSingle();

            Container.Bind<PlayerModel>().AsSingle().NonLazy();

            Container.BindInstance(_playerView).AsSingle();

            Container.BindFactory<float, Vector3, FloatingText, FloatingText.Factory>()
                .FromPoolableMemoryPool(poolBinder => poolBinder
                    .WithInitialSize(_poolInitialSize)
                    .FromComponentInNewPrefab(_floatingTextPrefab)
                    .UnderTransform(_poolParent)
                );

            Container.BindFactory<Vector3, ClickParticle, ClickParticle.Factory>()
                .FromPoolableMemoryPool(poolBinder => poolBinder
                    .WithInitialSize(_poolInitialSize)
                    .FromComponentInNewPrefab(_clickParticlePrefab)
                    .UnderTransform(_poolParent)
                );
            
            Container.Bind<PlayerPresenter>().AsSingle().NonLazy();
        }
    }
}