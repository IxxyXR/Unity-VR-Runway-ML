using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


public class Runway : MonoBehaviour
{

    private bool isProcessingInput;
    private ModelSession runningSession;
    private WebCamTexture webcamTexture;
    public Texture2D testTexture;
    public string url = "http://172.30.0.185:8080/shot.jpg";
    public string inputParameter;
    public string outputParameter;

    void Start()
    {
        StartCoroutine(GetTexture());
    }

    IEnumerator GetTexture()
    {
        while(true)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            testTexture = DownloadHandlerTexture.GetContent(www);
            var dataToSend = new Dictionary<string, object>();
            byte[] data;
            data = TextureToPNG(testTexture);
            dataToSend[inputParameter] = "data:image/png;base64," + Convert.ToBase64String(data);
            StartCoroutine(RunwayHub.runInference("http://localhost:8000", "query?", dataToSend, (outputData) =>
            {
                isProcessingInput = false;
                if (outputData == null) return;
                object value = outputData[outputParameter];
                string stringValue = value as string;
                int dataStartIndex = stringValue.IndexOf("base64,") + 7;
                byte[] outputImg = Convert.FromBase64String(((string) value).Substring(dataStartIndex));
                var tex = new Texture2D(2, 2); // Once image is loaded, texture will auto-resize
                tex.LoadImage(outputImg);
                GetComponent<MeshRenderer>().material.mainTexture = tex;
            }));
        }
    }

    public static byte[] TextureToPNG(Texture2D tex)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(
            tex.width,
            tex.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, tempRT);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tempRT;
        var tempTexture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB24, false);
        tempTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tempRT);
        return tempTexture.EncodeToPNG();
    }
}
