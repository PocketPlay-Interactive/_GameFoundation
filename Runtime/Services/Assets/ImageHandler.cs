using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageHandler : GenericSingleton<ImageHandler>
{
    public async Task DownloadImageAsync(string url, string fileName)
    {
        using (var www = UnityWebRequestTexture.GetTexture(url))
        {
            var op = www.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(www);
                byte[] itemBGBytes = texture.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + $"/{fileName}", itemBGBytes);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }
    }
}
