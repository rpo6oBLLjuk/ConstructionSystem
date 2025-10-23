using System.IO;
using UnityEngine;

public abstract class BaseSaver<T> : MonoBehaviour
{
    [SerializeField] protected string _saverName = "Saver";
    [SerializeField] protected string _baseDirectory = "Saves/";

    [SerializeField] protected string emptySaveName = "BaseSave";


    public virtual bool Save(T obj, string saveName, bool forceOverwrite = false)
    {
        if (!FileNameIsCorrect(saveName))
            return false;

        GetFullPath(saveName, out string fullPath);
        ConvertDataToJson(obj, out string json);

        if (File.Exists(fullPath))
        {
            if (!forceOverwrite)
            {
                //OverwritePopup ==========================================================================================================================
                return false;
            }
            else
            {
                OverwriteFile(true, fullPath, json);
                return true;
            }
        }
        else
        {
            File.WriteAllText(fullPath, json);

            //FileSaved Log

            return true;
        }
    }
    public virtual T Load(string saveName = default)
    {
        GetFullPath(saveName, out string fullPath);

        if (!File.Exists(fullPath))
        {
            //SaveNotFound Log
            return default;
        }
        else
        {
            string json = File.ReadAllText(fullPath);
            T data = ConvertJsonToData(json);

            //SaveLoaded Log

            return data;
        }
    }
    public virtual bool DeleteSave(string saveName)
    {
        if (!FileNameIsCorrect(saveName))
            return false;

        GetFullPath(saveName, out string fullPath);

        if (!File.Exists(fullPath))
        {
            ///SaveNotFound Log
            return false;
        }
        else
        {
            File.Delete(fullPath);

            //DeleteFile Log

            return true;
        }
    }

    protected void Awake()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, _baseDirectory);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);
    }

    protected void ConvertDataToJson(T data, out string json) => json = JsonUtility.ToJson(data, false);
    protected T ConvertJsonToData(string json) => JsonUtility.FromJson<T>(json);

    private void OverwriteFile(bool overwrite, string fullPath, string json)
    {
        if (overwrite)
        {
            File.WriteAllText(fullPath, json);
            //Overwrote Log
        }
        else
        {
            //NotOverwrote Log
        }
    }

    private bool FileNameIsCorrect(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            //Log
            return false;
        }
        return true;
    }
    private string GetFullPath(string saveName, out string fullPath) => fullPath = Path.Combine(Application.persistentDataPath, _baseDirectory, $"{saveName}.json");
}