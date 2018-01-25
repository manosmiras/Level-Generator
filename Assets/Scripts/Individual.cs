using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

[Serializable]
public class Individual : IEquatable<Individual>
{

    public List<DesignElement> designElements = new List<DesignElement>();
    public int fitness;

    public Individual()
    {

    }

    public Individual(Individual individual)
    {
        this.designElements = individual.designElements;
        this.fitness = individual.fitness;
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

    public bool Equals(Individual other)
    {
        int equalCount = 0;
        for (int i = 0; i < designElements.Count; i++)
        {
            // Sum the number of same design elements
            if (designElements[i].Equals(other.designElements[i]))
            {
                equalCount++;
            }
        }
        // Individuals are equal if all the design elements have the same values
        if (equalCount == designElements.Count)
            return true;
        return false;

    }
}
