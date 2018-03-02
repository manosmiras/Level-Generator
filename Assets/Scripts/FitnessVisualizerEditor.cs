using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FitnessVisualizerEditor : EditorWindow
{
    public static List<float> values = new List<float>();
    private float distance = 10;
    private float fittest = 0;
    private Vector2 fittestPos = new Vector2();
    [MenuItem("Window/Fitness Visualizer Editor Window")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FitnessVisualizerEditor));
    }

    private void OnGUI()
    {
        //Handles.color = Color.red;
        //Handles.DrawLine(new Vector2(0, 0), new Vector2(position.width, 0));
        //Handles.DrawLine(new Vector2(0, position.height - 1), new Vector2(position.width, position.height - 1));
        //GUI.Label(new Rect(position.width - 25, 0, 25, 25), "1");
        //GUI.Label(new Rect(position.width - 25, position.height - 25, 25, 25), "0");
        //GUI.TextField();
        fittest = 0;
        
        if (values.Count > 1)
        {
            //time += Time.deltaTime * 10f;
            if (values.Count * distance >= position.width)
                values.RemoveAt(0);
            Handles.color = Color.black;//Handles.color = new Color(1, 1, 1, 0.5f);
            for (int i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, position.height - (values[i] * position.height)), new Vector2(i * distance + distance, position.height - (values[i + 1] * position.height)));
                if (fittest <= values[i])
                {
                    fittest = values[i];
                    fittestPos = new Vector2(i * distance, position.height - (values[i] * position.height));
                }
                //GUI.Label(new Rect(i * distance, position.height - (values[i] * position.height), 22.5f, 25), values[i].ToString());
                //GUI.Label(new Rect(i * 20 + 20, values[i + 1] * position.height, 22.5f, 20), values[i].ToString());
            }

            
            //Handles.DrawLine(new Vector2(time, 50), new Vector2(time + 10, 50));
        }
        Repaint();
        GUI.contentColor = Color.red;
        GUI.backgroundColor = Color.red;
        GUI.Label(new Rect(fittestPos, new Vector2(25, 25)), "*");
    }

}
