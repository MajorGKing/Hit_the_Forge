using System;
using System.Collections;
using System.Net;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class CertificateWhore : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

public class WebManager
{
    public string BaseUrl { get; set; }
    public string ip = "127.0.0.1";
    public int port = 7777;
    
    public void Init()
    {
        IPAddress ipv4 = Utils.GetIpv4Address(ip);
        if (ipv4 == null)
        {
            Debug.LogError("WebServer IPv4 Failed");
            return;
        }
	
        BaseUrl = $"http://{ipv4.ToString()}:{port}";
        Debug.Log($"WebServer BaseUrl : {BaseUrl}");
    }

    // -------- Public API --------
    public async UniTask<T> SendPostRequestAsync<T>(string url, object obj)
    {
        return await SendWebRequestAsync<T>(url, UnityWebRequest.kHttpVerbPOST, obj);
    }

    public async UniTask<T> SendGetRequestAsync<T>(string url, object obj = null)
    {
        return await SendWebRequestAsync<T>(url, UnityWebRequest.kHttpVerbGET, obj);
    }

    // -------- Core WebRequest --------
    private async UniTask<T> SendWebRequestAsync<T>(string url, string method, object obj)
    {
        if (string.IsNullOrEmpty(BaseUrl))
            Init();

        //string sendUrl = $"{BaseUrl}/{url}";
        string sendUrl = url.StartsWith("http") ? url : $"{BaseUrl}/{url}";
        Debug.Log($"Call {sendUrl}");

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.certificateHandler = new CertificateWhore();
            uwr.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await uwr.SendWebRequest().ToUniTask();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[WebManager] Request Failed: {uwr.error}");
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebManager] Exception: {ex.Message}");
                return default;
            }
        }
    }
}
