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
        if (GUILayout.Button("Save fittest"))
        {
            GameObject levelGen = GameObject.FindGameObjectWithTag("LevelGenerator");
            LevelGenerator levelGenScript =  levelGen.GetComponent<LevelGenerator>();
            levelGenScript.fittestIndividual.ToJson();
        }
    }
}