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
                    Debug.Log($"[WebRequestService] GET {url}");
                    await webRequest.SendWebRequest().WithCancellation(token);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        return webRequest.downloadHandler?.text ?? string.Empty;
                    }

                    Debug.LogWarning($"[WebRequestService] Error: {webRequest.error}");
                    return null;
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[WebRequestService] Request cancelled");
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WebRequestService] Exception: {ex}");
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

        public CancellationToken CreateLinkedToken(CancellationToken external)
        {
            CancelCurrent();
            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(external);
            return _currentCts.Token;
        }

        public void Dispose()
        {
            CancelCurrent();
        }
    }
}
