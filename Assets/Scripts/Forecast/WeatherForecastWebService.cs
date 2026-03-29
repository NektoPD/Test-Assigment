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
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private CancellationTokenSource _currentRequestCts;
        private bool _isProcessing = false;
        private bool _isRunning = true;
        private UniTask _processingTask;

        private int _currentQueueSize = 0;

        public IObservable<RequestResult> RequestCompleted => _requestCompleted;
        private readonly Subject<RequestResult> _requestCompleted = new Subject<RequestResult>();
        
        public IObservable<Unit> RequestFailed => _requestFailed;
        private readonly Subject<Unit> _requestFailed = new Subject<Unit>();

        public WeatherForecastService(WeatherForecastConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
           // StartProcessing();
            //StartAutoRequests();
        }

        public void StartAutoRequests()
        {
            AutoRequestLoop(_cts.Token).Forget();
        }

        private async UniTaskVoid AutoRequestLoop(CancellationToken token)
        {
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
                    break;
                }
                catch (Exception ex)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), cancellationToken: token);
                }
            }
        }

        public void EnqueueRequest(string url)
        {
            if (!_isRunning)
            {
                return;
            }

            if (_currentQueueSize >= _config.MaxQueueSize)
            {
                return;
            }

            var requestItem = new RequestItem
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                EnqueueTime = DateTime.UtcNow
            };

            _requestQueue.Enqueue(requestItem);
            Interlocked.Increment(ref _currentQueueSize);
        }
        
        public void StopAllRequests()
        {
            _isRunning = false;

            _cts.Cancel();

            _currentRequestCts?.Cancel();

            while (_requestQueue.TryDequeue(out _))
            {
                Interlocked.Decrement(ref _currentQueueSize);
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts.Cancel();
            
            _currentRequestCts?.Cancel();
        }

        public void StartProcessing()
        {
            if (_isProcessing)
            {
                return;
            }

            _isProcessing = true;
            _processingTask = ProcessQueueLoop(_cts.Token);
        }

        private async UniTask ProcessQueueLoop(CancellationToken token)
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                if (_requestQueue.TryDequeue(out RequestItem request))
                {
                    Interlocked.Decrement(ref _currentQueueSize);

                    await ProcessRequestAsync(request, token);

                    if (_config.RequestIntervalSeconds > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds),
                            cancellationToken: token);
                    }
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds), cancellationToken: token);
                }
            }

            _isProcessing = false;
        }

        private async UniTask ProcessRequestAsync(RequestItem request, CancellationToken token)
        {
            using (_currentRequestCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            using (var webRequest = UnityWebRequest.Get(request.Url))
            {
                try
                {
                    await webRequest.SendWebRequest().WithCancellation(_currentRequestCts.Token);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        string jsonResponse = webRequest.downloadHandler?.text ?? string.Empty;
                        HandleSuccessResponse(jsonResponse);
                    }
                    else
                    {
                        HandleErrorResponse(webRequest.error, webRequest.downloadHandler?.text);
                    }
                }
                catch (OperationCanceledException)
                {
                    HandleErrorResponse("Request canceled", null);
                }
                catch (Exception ex)
                {
                    HandleErrorResponse(ex.Message, null);
                }
                finally
                {
                    _currentRequestCts.Dispose();
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
            Stop();
            _cts?.Dispose();

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