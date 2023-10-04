using UnityEngine;
using UnityEditor;
using System.IO;

public static class FileWriterManager
{
    public static string DefaultFileName = "test";

    public static string DefaultFilePath = "Assets/Resources/";

    public static void WriteString(string textContents, string FileName, string FilePath)
    {
        if (FilePath[FilePath.Length - 1] != '/')
        {
            FilePath += '/';
        }

        string path = FilePath + FileName;

        FileInfo fInfo = new FileInfo(path);

        if (!fInfo.Directory.Exists)
        {
            System.IO.Directory.CreateDirectory(fInfo.DirectoryName);
        }

        // Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path);
        writer.Write(textContents);
        writer.Close();
    }

    public static void WriteBinary(byte[] contents, string FileName, string FilePath)
    {
        // write all the bytes to the file
        string path = FilePath + FileName;

        System.IO.File.WriteAllBytes(path, contents);
    }

    [MenuItem("Tools/Read file")]
    public static void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

    public static string ReadString(string filePath)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(filePath);
        
        return reader.ReadToEnd();
    }
}
