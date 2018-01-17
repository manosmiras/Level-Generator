using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Save fittest"))
        {
            GameObject levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
            LevelGenerator levelGenScript =  levelGen.GetComponent<LevelGenerator>();
            levelGenScript.fittestIndividual.ToJson();
        }
    }
}