using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AddTFromRT : MonoBehaviour
{
    [SerializeField]
    TextureFormat format = TextureFormat.RGBA64_SIGNED;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] private Texture2D savedTexture;

    [SerializeField] private RawImage image;


    private void Awake() => SaveRenderTextureToTexture2D();

    private void SaveRenderTextureToTexture2D()
    {
        RenderTexture rt = renderTexture;
        RenderTexture rtNew = new(rt.width, rt.height, 8, RenderTextureFormat.ARGB32); //fix sRGB dark img https://gist.github.com/krzys-h/76c518be0516fb1e94c7efbdcd028830?permalink_comment_id=5099168#gistcomment-5099168
        Graphics.Blit(rt, rtNew);

        RenderTexture.active = rtNew;

        Texture2D savedTexture = new(rt.width, rt.height, format, false, true);
        savedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = savedTexture.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, "TestSave.png");
        System.IO.File.WriteAllBytes(path, bytes);
        //AssetDatabase.ImportAsset(path);
        Debug.Log("Saved to " + path);

        image.texture = LoadFromFile();
    }

    //public void SaveToFile()
    //{
    //    if (renderTexture == null)
    //        return;

    //    // Конвертируем в PNG bytes
    //    byte[] bytes = savedTexture.EncodeToPNG();

    //    // Сохраняем в файл
    //    string path = Path.Combine(Application.persistentDataPath, "TestSave.png");
    //    File.WriteAllBytes(path, bytes);

    //    Debug.Log($"Текстура сохранена: {path}");
    //}

    public Texture2D LoadFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "TestSave.png");
        byte[] bytes = File.ReadAllBytes(path);


        Texture2D texture = new Texture2D(512, 512);
        if (texture.LoadImage(bytes))
        {
            return texture;
        }
        else
        {
            Destroy(texture);
            return null;
        }
    }
}
