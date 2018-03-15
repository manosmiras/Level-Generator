using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FitnessVisualizerEditor : EditorWindow
{
    public static List<float> values = new List<float>();
    public static List<float> values2 = new List<float>();
    public static List<float> values3 = new List<float>();
    public static Technique technique;
    private float distance = 20;

    [MenuItem("Window/Fitness Visualizer Editor Window")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FitnessVisualizerEditor));
    }

    private void OnGUI()
    {
        Handles.color = Color.black;

        switch (technique)
        {
            case Technique.SimpleGA:
                GUI.Label(new Rect(0, 0, 125, 25), "Fitness");
                SingleGraph();
                break;
            case Technique.FI2PopGA:
                GUI.Label(new Rect(0, 0, 200, 25), "Infeasible Population Fitness");
                GUI.Label(new Rect(0, position.height / 2, 200, 25), "Feasible Population Fitness");
                DoubleGraph();
                break;
            case Technique.NoveltySearchGA:
                GUI.Label(new Rect(0, 0, 125, 25), "Fitness");
                GUI.Label(new Rect(0, position.height / 2, 125, 25), "Novelty");
                DoubleGraph();
                break;
            case Technique.FI2PopNsGA:
                GUI.Label(new Rect(0, 0, 125, 25), "Infeasible Population Fitness");
                GUI.Label(new Rect(0, position.height / 3, 200, 25), "Feasible Population Fitness");
                GUI.Label(new Rect(0, (position.height / 3) * 2, 200, 25), "Feasible Population Novelty");
                TripleGraph();
                break;
            default:
                break;
        }

        Repaint();
    }

    private void SingleGraph()
    {
        if (values.Count > 1)
        {
            if (values.Count * distance >= position.width)
                values.RemoveAt(0);

            for (int i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, position.height - (values[i] * position.height)), new Vector2(i * distance + distance, position.height - (values[i + 1] * position.height)));
                GUI.Label(new Rect(i * distance, position.height - (values[i] * position.height), 22.5f, 25), values[i].ToString());
            }

        }
    }

    private void DoubleGraph()
    {
        if (values2.Count > 1)
        {
            if (values2.Count * distance >= position.width)
                values2.RemoveAt(0);
            for (int i = 0; i < values2.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, ((1 - values2[i]) * (position.height / 2))), new Vector2(i * distance + distance, ((1 - values2[i + 1]) * (position.height / 2))));
                GUI.Label(new Rect(i * distance, ((1 - values2[i]) * (position.height / 2)), 22.5f, 25), values2[i].ToString());
            }
        }

        if (values.Count > 1)
        {
            if (values.Count * distance >= position.width)
                values.RemoveAt(0);

            for (int i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, (position.height / 2) + ((1 - values[i]) * (position.height / 2))), new Vector2(i * distance + distance, (position.height / 2) + ((1 - values[i + 1]) * (position.height / 2))));
                GUI.Label(new Rect(i * distance, (position.height / 2) + ((1 - values[i]) * (position.height / 2)), 22.5f, 25), values[i].ToString());
            }
        }
    }

    private void TripleGraph()
    {
        if (values2.Count > 1)
        {
            if (values2.Count * distance >= position.width)
                values2.RemoveAt(0);
            for (int i = 0; i < values2.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, ((1 - values2[i]) * (position.height / 3))), new Vector2(i * distance + distance, ((1 - values2[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * distance, ((1 - values2[i]) * (position.height / 3)), 22.5f, 25), values2[i].ToString());
            }
        }

        if (values.Count > 1)
        {
            if (values.Count * distance >= position.width)
                values.RemoveAt(0);

            for (int i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, (position.height / 3) + ((1 - values[i]) * (position.height / 3))), new Vector2(i * distance + distance, (position.height / 3) + ((1 - values[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * distance, (position.height / 3) + ((1 - values[i]) * (position.height / 3)), 22.5f, 25), values[i].ToString());
            }
        }

        if (values3.Count > 1)
        {
            if (values3.Count * distance >= position.width)
                values3.RemoveAt(0);

            for (int i = 0; i < values3.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * distance, ((position.height / 3) * 2) + ((1 - values3[i]) * (position.height / 3))), new Vector2(i * distance + distance, ((position.height / 3) * 2) + ((1 - values3[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * distance, ((position.height / 3) * 2) + ((1 - values3[i]) * (position.height / 3)), 22.5f, 25), values3[i].ToString());
            }
        }
    }

}
