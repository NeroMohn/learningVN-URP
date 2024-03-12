using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{

    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if(!filePath.StartsWith("/"))
            filePath = FilePaths.root + filePath;

        List<string> lines = new List<string>();

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch(FileNotFoundException e) 
        {
            Debug.LogError($"File not found: '{e.FileName}'");
        }

        return lines;
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        return null;
    }


}
