using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Forecast
{
    public class WeatherForecastService : IDisposable
    {
        private readonly WeatherForecastConfig _config;
        private readonly WebRequestService _web;

        private CancellationTokenSource _mainCts;
        private bool _isRunning;

        public IObservable<RequestResult> RequestCompleted => _requestCompleted;
        private readonly Subject<RequestResult> _requestCompleted = new Subject<RequestResult>();

        public IObservable<Unit> RequestFailed => _requestFailed;
        private readonly Subject<Unit> _requestFailed = new Subject<Unit>();

        public WeatherForecastService(WeatherForecastConfig config, WebRequestService web)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _web = web ?? throw new ArgumentNullException(nameof(web));
        }

        public void StartAllRequests()
        {
            StopAllRequests();

            _isRunning = true;
            _mainCts = new CancellationTokenSource();

            AutoRequestLoop(_mainCts.Token).Forget();
        }

        public void StopAllRequests()
        {
            _isRunning = false;
            _mainCts?.Cancel();
            _mainCts?.Dispose();
            _mainCts = null;
        }

        private async UniTaskVoid AutoRequestLoop(CancellationToken token)
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    var json = await _web.GetAsync(_config.PublicApi, token);

                    if (json != null)
                    {
                        _requestCompleted.OnNext(new RequestResult
                        {
                            Json = json,
                            Success = true,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        _requestFailed.OnNext(Unit.Default);
                        _requestCompleted.OnNext(new RequestResult
                        {
                            Json = string.Empty,
                            Success = false,
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds),
                        cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    _requestFailed.OnNext(Unit.Default);

                    if (_isRunning && !token.IsCancellationRequested)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.RequestIntervalSeconds),
                            cancellationToken: token);
                    }
                }
            }
        }

        public void Dispose()
        {
            StopAllRequests();
            _requestCompleted.Dispose();
            _requestFailed.Dispose();
        }
    }
}

public struct RequestResult
{
    public string Json;
    public bool Success;
    public DateTime Timestamp;
}