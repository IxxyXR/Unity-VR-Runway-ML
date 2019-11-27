using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebcamView : MonoBehaviour
{

    public GameObject screen;
    public string url = "http://172.30.2.137:8080/shot.jpg";

    private Material mat;

    void Start()
    {
        mat = screen.GetComponent<MeshRenderer>().material;
        StartCoroutine(GetTexture());
    }

    IEnumerator GetTexture()
    {
        while(true)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            Texture tex = DownloadHandlerTexture.GetContent(www);
            mat.mainTexture = tex;
        }
    }
}
