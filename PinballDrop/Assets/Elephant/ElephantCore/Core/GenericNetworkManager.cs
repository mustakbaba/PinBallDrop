using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ElephantSDK
{
    public class GenericNetworkManager<T> where T : new()
    {
        private readonly string _gameID;
        private readonly string _gameSecret;

        public GenericNetworkManager()
        {
            _gameID = ElephantCore.Instance.gameID;
            _gameSecret = ElephantCore.Instance.gameSecret;
        }

        public IEnumerator PostWithResponse(string url, string bodyJsonString, Action<GenericResponse<T>> onResponse,
            Action<string> onError, float? timeout = null, bool isPut = false)
        {
            using (var request = CreateRequest(url, bodyJsonString, isPut))
            {
                var operation = request.SendWebRequest();
                var startTime = Time.time;
                while (!operation.isDone)
                {
                    if (timeout.HasValue && Time.time - startTime > timeout.Value)
                    {
                        onError?.Invoke("Request timed out.");
                        yield break;
                    }

                    yield return null;
                }

                ProcessResponse(request, onResponse, onError, bodyJsonString);
            }
        }

        //TODO: Merge this with PostWithResponse when gamekit release
        public IEnumerator PostWithResponseSocial(string url, string bodyJsonString,
            Action<GenericResponse<T>> onResponse,
            Action<string> onError, int timeout = 0, bool isPut = false,
            Action<UnityWebRequest> onFailedResponse = null)
        {
            using (var request = CreateRequest(url, bodyJsonString, isPut))
            {
                request.timeout = timeout;
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                }

                ProcessResponseSocial(request, onResponse, onError, bodyJsonString, onFailedResponse);
            }
        }

        public IEnumerator DownloadTexture(string url, Action<Texture2D> onSuccess, Action<string> onError)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture == null)
                        {
                            onError?.Invoke("Failed to download the texture");
                            yield break;
                        }

                        byte[] imageData = texture.EncodeToPNG();
                        Utils.SaveImageToFileInSubdirectory(Utils.GetFileNameFromUrl(url), imageData);
                        onSuccess?.Invoke(texture);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Error processing the image: {ex.Message}");
                    }
                }
            }
        }

        private UnityWebRequest CreateRequest(string url, string bodyJsonString, bool isPut = false)
        {
            var request = isPut
                ? new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT)
                : new UnityWebRequest(url,
                    bodyJsonString != null ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbGET);

            if (bodyJsonString != null)
            {
                var bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Content-Encoding", "gzip");
            request.SetRequestHeader("Authorization", Utils.SignString(bodyJsonString, _gameSecret));
            request.SetRequestHeader("GameID", _gameID);

            return request;
        }

        private void ProcessResponse(UnityWebRequest request, Action<GenericResponse<T>> onResponse,
            Action<string> onError, string bodyJsonString)
        {
            LogRequest(request, bodyJsonString);
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                LogRequest(request, bodyJsonString);
                onError?.Invoke("Request Error");
            }
            else
            {
                try
                {
                    onResponse?.Invoke(new GenericResponse<T>(request));
                }
                catch (Exception e)
                {
                    onError?.Invoke(e.Message);
                }
            }
        }

        private void ProcessResponseSocial(UnityWebRequest request, Action<GenericResponse<T>> onResponse,
            Action<string> onError, string bodyJsonString, Action<UnityWebRequest> onFailedResponse = null)
        {
            LogRequest(request, bodyJsonString);
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    onError?.Invoke(request.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    onFailedResponse?.Invoke(request);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    onError?.Invoke(request.error);
                    break;
                default:
                    try
                    {
                        onResponse?.Invoke(new GenericResponse<T>(request));
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke(e.Message);
                    }

                    break;
            }
        }

        private static void LogRequest(UnityWebRequest request, string bodyJsonString)
        {
            ElephantLog.Log("WEB_REQUEST", "URL: " + request.url);
            ElephantLog.Log("WEB_REQUEST", "BODY:" + bodyJsonString);
            ElephantLog.Log("WEB_REQUEST", "RES CODE:" + request.responseCode.ToString());
            if (request.downloadHandler != null)
            {
                ElephantLog.Log("WEB_REQUEST", "RES BODY:" + request.downloadHandler.text);
            }
        }
    }
}