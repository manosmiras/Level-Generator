using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public Texture2D image;
    public static bool load = false;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (load)
        {
            image = Resources.Load("FittestLevel.png", typeof(Texture2D)) as Texture2D;
            load = false;
        }
        if (image != null)
        {
            GUILayout.Label(image); //Or draw the texture
        }
        else
        {
            Debug.Log("image is null");
        }

        if (GUILayout.Button("Save fittest"))
        {
            GameObject levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
            LevelGenerator levelGenScript =  levelGen.GetComponent<LevelGenerator>();
            levelGenScript.fittestIndividual.ToJson();
        }
    }
}