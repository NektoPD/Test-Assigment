using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Forecast
{
    public class WeatherForecastService : IDisposable
    {
        private readonly WeatherForecastConfig _config;
        private readonly ConcurrentQueue<RequestItem> _requestQueue = new ConcurrentQueue<RequestItem>();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private CancellationTokenSource _mainCts;
        private CancellationTokenSource _currentRequestCts;
        private bool _isRunning = false;
        private UniTask _processingTask;

        private int _currentQueueSize = 0;

        public IObservable<RequestResult> RequestCompleted => _requestCompleted;
        private readonly Subject<RequestResult> _requestCompleted = new Subject<RequestResult>();

        public IObservable<Unit> RequestFailed => _requestFailed;
        private readonly Subject<Unit> _requestFailed = new Subject<Unit>();

        public WeatherForecastService(WeatherForecastConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private void StartAutoRequests()
        {
            AutoRequestLoop(_mainCts.Token).Forget();
        }

        private async UniTaskVoid AutoRequestLoop(CancellationToken token)
        {
            Debug.Log("[AutoRequestLoop] Started");

            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    EnqueueRequest(_config.PublicApi);
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), 
                        cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[AutoRequestLoop] Cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AutoRequestLoop] Error: {ex}");
                    if (_isRunning && !token.IsCancellationRequested)
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), cancellationToken: token);
                }
            }
        
            Debug.Log("[AutoRequestLoop] Exited");
        }

        public void EnqueueRequest(string url)
        {
            if (!_isRunning) return;
            if (_currentQueueSize >= _config.MaxQueueSize) return;

            var requestItem = new RequestItem
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                EnqueueTime = DateTime.UtcNow
            };

            _requestQueue.Enqueue(requestItem);
            Interlocked.Increment(ref _currentQueueSize);
        
            Debug.Log($"[Enqueue] Request added. Queue size: {_currentQueueSize}");
        }

        public void StartAllRequests()
        {
            StopAllRequests();

            _isRunning = true;

            _mainCts?.Cancel();
            _mainCts?.Dispose();
            _mainCts = new CancellationTokenSource();

            StartProcessing();
            StartAutoRequests();

            EnqueueRequest(_config.PublicApi);
        }

        public void StopAllRequests()
        {
            _isRunning = false;
    
            _mainCts?.Cancel();
            _currentRequestCts?.Cancel();

            while (_requestQueue.TryDequeue(out _))
            {
                Interlocked.Decrement(ref _currentQueueSize);
            }
            _currentQueueSize = 0;
        }
        
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts.Cancel();

            _currentRequestCts?.Cancel();
        }

        private void StartProcessing()
        {
            ProcessQueueLoop(_mainCts.Token).Forget();
        }

        private async UniTaskVoid ProcessQueueLoop(CancellationToken token)
        {
            Debug.Log("[ProcessQueueLoop] Started");

            while (_isRunning && !token.IsCancellationRequested)
            {
                if (_requestQueue.TryDequeue(out RequestItem request))
                {
                    Interlocked.Decrement(ref _currentQueueSize);
                
                    Debug.Log($"[ProcessQueue] Dequeued request {request.Id}");
                    await ProcessRequestAsync(request, token);
                
                    if (_config.RequestIntervalSeconds > 0 && _isRunning && !token.IsCancellationRequested)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), cancellationToken: token);
                    }
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), cancellationToken: token);
                }
            }

            Debug.Log("[ProcessQueueLoop] Exited");
        }

        private async UniTask ProcessRequestAsync(RequestItem request, CancellationToken token)
        {
            using (_currentRequestCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            using (var webRequest = UnityWebRequest.Get(request.Url))
            {
                try
                {
                    Debug.Log($"[ProcessRequestAsync] Starting request to {request.Url}");
                
                    await webRequest.SendWebRequest().WithCancellation(_currentRequestCts.Token);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        string json = webRequest.downloadHandler?.text ?? string.Empty;
                        Debug.Log("[ProcessRequestAsync] Success");
                        HandleSuccessResponse(json);
                    }
                    else
                    {
                        Debug.LogWarning($"[ProcessRequestAsync] Web error: {webRequest.error}");
                        HandleErrorResponse(webRequest.error, webRequest.downloadHandler?.text);
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[ProcessRequestAsync] Cancelled");
                    HandleErrorResponse("Request canceled", null);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ProcessRequestAsync] Exception: {ex}");
                    HandleErrorResponse(ex.Message, null);
                }
                finally
                {
                    _currentRequestCts?.Dispose();
                    _currentRequestCts = null;
                }
            }
        }

        private void HandleSuccessResponse(string json)
        {
            var result = new RequestResult
            {
                Json = json,
                Success = true,
                Timestamp = DateTime.UtcNow
            };

            _requestCompleted.OnNext(result);
        }

        private void HandleErrorResponse(string errorMessage, string rawJson)
        {
            _requestFailed.OnNext(Unit.Default);

            var result = new RequestResult
            {
                Json = rawJson ?? string.Empty,
                Success = false,
                Timestamp = DateTime.UtcNow
            };

            _requestCompleted.OnNext(result);
        }

        public void Dispose()
        {
            StopAllRequests();
            _mainCts?.Dispose();
            _requestCompleted.Dispose();
            _requestFailed.Dispose();
        }

        private class RequestItem
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public DateTime EnqueueTime { get; set; }
        }
    }
}

public struct RequestResult
{
    public string Json;
    public bool Success;
    public DateTime Timestamp;
}