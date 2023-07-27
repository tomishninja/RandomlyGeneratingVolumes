using UnityEngine;

[System.Serializable]
public class JSONFileReaderWriter<T> : FileReaderWriter<T>
{
    public JSONFileReaderWriter(string pathToFile)
    {
        this.pathToFile = pathToFile;
    }

    public override T ReadFile()
    {
        string input = System.IO.File.ReadAllText(pathToFile);
        return JsonUtility.FromJson<T>(input);
    }

    public override void WriteFile(T dataToWrite)
    {
        string json = JsonUtility.ToJson(this, true);
        System.IO.File.WriteAllText(pathToFile, json);
    }
}
