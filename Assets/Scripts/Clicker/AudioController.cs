using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Zenject;
using System.Linq;

namespace Clicker
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSourcePrefab;
        [SerializeField] private AudioClip[] _clickClips;
        [SerializeField] private int _poolSize = 5;
        [SerializeField] private float _maxPlayTime = 2f;

        private List<AudioSource> _audioSourcePool = new List<AudioSource>();
        private List<AudioSource> _activeSources = new List<AudioSource>();
        
        private void Awake()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                AudioSource newSource = Instantiate(_audioSourcePrefab, transform);
                newSource.gameObject.SetActive(false);
                _audioSourcePool.Add(newSource);
            }
        }
        
        public void PlayClickSound()
        {
            AudioSource source = GetAvailableAudioSource();
            
            if (source != null)
            {
                int randomClipIndex = Random.Range(0, _clickClips.Length);
                source.clip = _clickClips[randomClipIndex];
                source.Play();

                _activeSources.Add(source);

                StartCoroutine(ReturnToPoolAfterPlay(source));
            }
        }
        
        private AudioSource GetAvailableAudioSource()
        {
            AudioSource freeSource = _audioSourcePool.FirstOrDefault(s => !s.gameObject.activeSelf);
            
            if (freeSource != null)
            {
                freeSource.gameObject.SetActive(true);
                return freeSource;
            }

            if (_activeSources.Count > 0)
            {
                AudioSource oldestSource = _activeSources[0];
                oldestSource.Stop();
                ReturnToPool(oldestSource);
                return GetAvailableAudioSource();
            }
            
            return null;
        }
        
        private IEnumerator ReturnToPoolAfterPlay(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);

            float waitTime = Mathf.Min(source.clip.length, _maxPlayTime);
            yield return new WaitForSeconds(waitTime);
            
            ReturnToPool(source);
        }
        
        private void ReturnToPool(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.gameObject.SetActive(false);
                _activeSources.Remove(source);
            }
        }
        
        private void OnDestroy()
        {
            foreach (var source in _audioSourcePool)
            {
                if (source != null)
                    Destroy(source.gameObject);
            }
            _audioSourcePool.Clear();
            _activeSources.Clear();
        }
    }
}