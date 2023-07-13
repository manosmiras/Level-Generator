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
    [SerializeField]
    public float fitness;
    [SerializeField]
    public bool delete;
    public Individual()
    {
        delete = false;
        fitness = 0;
    }

    public Individual(Individual individual)
    {
        designElements = new List<DesignElement>(individual.designElements);
        fitness = individual.fitness;
        delete = false;
    }

    public Individual(List<DesignElement> designElements)
    {
        this.designElements = designElements;
        fitness = 0;
    }
    // Sort by x axis
    public void Sort()
    {
        designElements = designElements.OrderBy(t => t.position.y).ThenBy(t => t.position.x).ToList();
    }


    public Individual DeepCopy()
    {
        var other = (Individual)MemberwiseClone();
        other.designElements = new List<DesignElement>(this.designElements);
        other.fitness = fitness;
        return other;
    }

    public float GetDiversity(Individual other)
    {
        var diversity = 0;
        for (var i = 0; i < designElements.Count; i++)
        {
            // Increase diversity if rotations for current piece are different
            if (designElements[i].rotation != other.designElements[i].rotation)
            {
                diversity++;
            }
            // Increase diversity if current piece types are different
            if (((LevelPiece)designElements[i]).type != ((LevelPiece)other.designElements[i]).type)
            {
                diversity++;
            }
        }
        // Normalize diversity
        var minDiversity = 0;
        var maxDiversity = designElements.Count * 2;
        var diversityNormalized = (diversity - minDiversity) / (maxDiversity - minDiversity);
        return diversityNormalized;
    }

    public float GetLinearity()
    {
        var ints = new int[designElements.Count];
        var count = 0;
        foreach (LevelPiece levelPiece in designElements)
        {

            var num = (int)levelPiece.type;
            ints[count] = num;
            //ints.Add(num);
            count++;
        }
        var query = ints.GroupBy(r => r)
        .Select(grp => new
        {
            Value = grp.Key,
            Count = grp.Count()
        });
        float min = 15 / designElements.Count;
        float max = 0;
        float maxVal = 0;
        foreach (var item in query)
        {
            if (item.Count > max)
            {
                max = item.Count;
                maxVal = item.Value;
            }
        }
        float linearity = 0;
        if (max > min)
            linearity = max / designElements.Count;
        return linearity;
    }

    public void Print()
    {
        var output = "";
        foreach (var de in designElements)
        {
            output += "Position:" + de.position + ", Rotation: " + de.rotation + "\n";

        }
        Debug.Log(output);
    }

    public void ToJson()
    {
        var json = JsonConvert.SerializeObject(this);
        var path = Application.dataPath + "/Levels/" + "gl" + designElements.Count + "f" + fitness + ".json";
        Debug.Log(path);
        File.WriteAllText(path, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public void ToJson(string text)
    {
        var json = JsonConvert.SerializeObject(this);
        var path = Application.dataPath + "/Levels/" + "gl" + designElements.Count + "f" + fitness + text + ".json";
        Debug.Log(path);
        File.WriteAllText(path, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public static Individual FromJson(string path)
    {
        var json = File.ReadAllText(path);
        JsonConverter[] converters = { new LevelPieceConverter() };

        var individual = JsonConvert.DeserializeObject<Individual>(json, new JsonSerializerSettings() { Converters = converters });
        
        return individual;
    }

    public bool Equals(Individual other)
    {
        if (other.designElements.Count != 0)
        {
            var equalCount = 0;
            for (var i = 0; i < designElements.Count; i++)
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
