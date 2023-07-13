using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public Texture2D image;
    public static bool load = false;

    private LevelGenerator _levelGenerator;
    private List<string> _dontInclude = new();

    private void OnEnable()
    {
        var levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
        _levelGenerator = levelGen.GetComponent<LevelGenerator>();
    }

    public override void OnInspectorGUI()
    {
        _dontInclude = new List<string> { "terminate" };
        if (_levelGenerator.technique != Technique.NoveltySearchGA)
        {
            _dontInclude.Add("minimalCriteria");
            _dontInclude.Add("deathPenalty");
        }

        if (_levelGenerator.technique == Technique.NoveltySearchGA || _levelGenerator.technique == Technique.SimpleGA)
        {
            _dontInclude.Add("currentFeasibleIndividual");
            _dontInclude.Add("currentInfeasibleIndividual");
        }
        else
        {
            _dontInclude.Add("currentIndividual");
        }

        if (!_levelGenerator.testing)
        {
            _dontInclude.Add("testName");
            _dontInclude.Add("testRuns");
        }


        DrawPropertiesExcluding(serializedObject, _dontInclude.ToArray());
        serializedObject.ApplyModifiedProperties();
        if (load)
        {
            // Load fittest level image
            image = Resources.Load("FittestLevel", typeof(Texture2D)) as Texture2D;
            load = false;
        }
        if (image != null)
        {
            GUILayout.Label(image); //Or draw the texture
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
            _levelGenerator.terminate = true;
        }
    }
}