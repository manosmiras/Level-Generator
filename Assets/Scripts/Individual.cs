using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using Newtonsoft.Json;

[Serializable]
public class Individual : IEquatable<Individual>
{

    public List<DesignElement> designElements = new List<DesignElement>();
    // fitness is used a form of measurement for the feasibility of 
    public int fitness;
    public int objectiveFitness;

    public Individual()
    {

    }

    public Individual(Individual individual)
    {
        this.designElements = new List<DesignElement>(individual.designElements);
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


    public Individual DeepCopy()
    {
        Individual other = (Individual)this.MemberwiseClone();
        other.designElements = new List<DesignElement>(this.designElements);
        other.fitness = this.fitness;
        return other;
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
        string json = JsonConvert.SerializeObject(this);
        
       
        string path = Application.dataPath + "/Levels/" + "gl" + designElements.Count + "f" + fitness + ".json";
        Debug.Log(path);
        File.WriteAllText(path, json.ToString());
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public void ToJson(string text)
    {
        string json = JsonConvert.SerializeObject(this);


        string path = Application.dataPath + "/Levels/" + "gl" + designElements.Count + "f" + fitness + text + ".json";
        Debug.Log(path);
        File.WriteAllText(path, json.ToString());
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public static Individual FromJson(string path)
    {
        string json = File.ReadAllText(path);
        JsonConverter[] converters = { new LevelPieceConverter() };

        Individual individual = JsonConvert.DeserializeObject<Individual>(json, new JsonSerializerSettings() { Converters = converters });
        
        return individual;
    }

    public bool Equals(Individual other)
    {
        if (other.designElements.Count != 0)
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
        // Other is not initialised properly - they are not equal
        return false;
    }
}
