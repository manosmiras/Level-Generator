using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public Texture2D image;
    public static bool load = false;
    LevelGenerator levelGenerator;
    //private static string[] dontInclude = new string[7];
    List<string> dontInclude = new List<string>();

    private void OnEnable()
    {
        GameObject levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
        levelGenerator = levelGen.GetComponent<LevelGenerator>();
    }

    public override void OnInspectorGUI()
    {
        dontInclude = new List<string>();
        dontInclude.Add("terminate");
        if (levelGenerator.technique != Technique.NoveltySearchGA)
        {
            dontInclude.Add("minimalCriteria");
            dontInclude.Add("deathPenalty");
        }

        if (levelGenerator.technique == Technique.NoveltySearchGA || levelGenerator.technique == Technique.SimpleGA)
        {
            dontInclude.Add("currentFeasibleIndividual");
            dontInclude.Add("currentInfeasibleIndividual");
        }
        else
        {
            dontInclude.Add("currentIndividual");
        }

        if (!levelGenerator.testing)
        {
            dontInclude.Add("testName");
            dontInclude.Add("testRuns");
        }


        DrawPropertiesExcluding(serializedObject, dontInclude.ToArray());
        serializedObject.ApplyModifiedProperties();
        
        //DrawDefaultInspector();

        if (load)
        {
            // Load fittest level image
            image = Resources.Load("FittestLevel", typeof(Texture2D)) as Texture2D;
            // Scale down image size
            //image.Resize(100, 100, image.format, true);
            //image.height = 100;
            //image.width = 100;
            load = false;
        }
        if (image != null)
        {
            GUILayout.Label(image); //Or draw the texture
        }
        else
        {
            //Debug.Log("image is null");
        }

        // Save fittest level in json
        //if (GUILayout.Button("Save fittest"))
        //{
        //    GameObject levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
        //    LevelGenerator levelGenScript =  levelGen.GetComponent<LevelGenerator>();
        //    //levelGenScript.fittestIndividual.ToJson();
        //}
        if (GUILayout.Button("Terminate"))
        {
            levelGenerator.terminate = true;
        }
    }
}