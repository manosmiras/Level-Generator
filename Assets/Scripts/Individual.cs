using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

[Serializable]
public class Individual
{
    
    public List<DesignElement> designElements = new List<DesignElement>();
    public int fitness;

    public Individual()
    {
        
    }

    public Individual(List<DesignElement> designElements)
    {
        this.designElements = designElements;
    }
    // Sort by x axis
    public void Sort()
    {
        designElements = designElements.OrderBy(t => t.position.y).ThenBy(t => t.position.x).ToList();
    }

    public void Print()
    {
        string output = "";
        foreach (DesignElement de in designElements)
        {
            output += "Position:" + de.position + ", Rotation: " + de.rotation + "\n";
            
        }
        Debug.Log(output);
    }

    public void ToJson()
    {
        string json = "";
        //string json = JsonUtility.ToJson(designElements);
        json += "{";
        json += "\n   FITNESS: " + fitness + "\n";
        foreach (LevelPiece lp in designElements)
        {
            json += "   Position: " + lp.position + ",\n   Rotation: " + lp.rotation + ",\n   Type: " + lp.type + "\n";
        }
        json += "}";

        string path = Application.dataPath + "/" + "gl" + designElements.Count + "f" + fitness + ".json";
        Debug.Log("AssetPath:" + path);
        File.WriteAllText(path, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }



}
