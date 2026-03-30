using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public class WebRequestService : IDisposable
    {
        private CancellationTokenSource _currentCts;

        public async UniTask<string> GetAsync(string url, CancellationToken token = default)
        {
            using (var webRequest = UnityWebRequest.Get(url))
            {
                try
                {
                    await webRequest.SendWebRequest().WithCancellation(token);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log(webRequest.downloadHandler);
                        
                        return webRequest.downloadHandler?.text ?? string.Empty;
                    }

                    return null;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public void CancelCurrent()
        {
            _currentCts?.Cancel();
            _currentCts?.Dispose();
            _currentCts = null;
        }

        public void Dispose()
        {
            CancelCurrent();
        }
    }
}
