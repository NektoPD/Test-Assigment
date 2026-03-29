using UnityEngine;
using Zenject;
using UniRx;
using System;

namespace Clicker
{
    public class ClickParticle : MonoBehaviour, IPoolable<Vector3, IMemoryPool>, IDisposable
    {
        [SerializeField] private ParticleSystem _particleSystem;
        
        [SerializeField] private bool _useRandomOffset = true;
        [SerializeField] private float _offsetRadius = 0.3f;
        [SerializeField] private bool _useCircularOffset = true;
        
        private IMemoryPool _pool;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private void Awake()
        {
            if (_particleSystem == null)
                _particleSystem = GetComponent<ParticleSystem>();

            gameObject.SetActive(false);
        }
        
        public void OnSpawned(Vector3 position, IMemoryPool pool)
        {
            gameObject.SetActive(true);
            
            _pool = pool;

            Vector3 finalPosition = CalculateOffsetPosition(position);
            transform.position = finalPosition;
            
            if (_particleSystem != null)
            {
                _disposables.Clear();
                
                Observable.EveryUpdate()
                    .Where(_ => !_particleSystem.isPlaying)
                    .First()
                    .Subscribe(_ => ReturnToPool())
                    .AddTo(_disposables);
                
                _particleSystem.Play();
            }
            else
            {
                ReturnToPool();
            }
        }
        
        private Vector3 CalculateOffsetPosition(Vector3 basePosition)
        {
            if (!_useRandomOffset)
                return basePosition;
            
            if (_useCircularOffset)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * _offsetRadius;
                return basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            }
            else
            {
                float offsetX = UnityEngine.Random.Range(-_offsetRadius, _offsetRadius);
                float offsetY = UnityEngine.Random.Range(-_offsetRadius, _offsetRadius);
                return basePosition + new Vector3(offsetX, offsetY, 0);
            }
        }
        
        public void OnDespawned()
        {
            _disposables.Clear();
            _pool = null;

            gameObject.SetActive(false);
        }
        
        private void ReturnToPool()
        {
            if (_pool != null)
            {
                _pool.Despawn(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
        
        public class Factory : PlaceholderFactory<Vector3, ClickParticle>
        {
        }
    }
}