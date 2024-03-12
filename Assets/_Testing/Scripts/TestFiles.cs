using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFiles : MonoBehaviour
{
    [SerializeField]
    private string fileName = "testFile.txt";


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        List <string> lines = FileManager.ReadTextFile(fileName);

        foreach (string line in lines)
        {
            Debug.Log(line);
        }

        yield return null;
    }
}
