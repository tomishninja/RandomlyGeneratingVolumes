using UnityEngine;

[System.Serializable]
public abstract class FileReaderWriter<T>
{
    [SerializeField] protected string pathToFile = "backup";

    public abstract T ReadFile();

    public abstract void WriteFile(T dataToWrite);
}
