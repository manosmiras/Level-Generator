using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FitnessVisualizerEditor : EditorWindow
{
    public static List<float> values = new();
    public static List<float> values2 = new();
    public static List<float> values3 = new();
    public static Technique technique;
    private const float Distance = 20;

    [MenuItem("Window/Fitness Visualizer Editor Window")]
    private static void Init()
    {
        GetWindow(typeof(FitnessVisualizerEditor));
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
            if (values.Count * Distance >= position.width)
                values.RemoveAt(0);

            for (var i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, position.height - (values[i] * position.height)), new Vector2(i * Distance + Distance, position.height - (values[i + 1] * position.height)));
                GUI.Label(new Rect(i * Distance, position.height - (values[i] * position.height), 22.5f, 25), values[i].ToString());
            }

        }
    }

    private void DoubleGraph()
    {
        if (values2.Count > 1)
        {
            if (values2.Count * Distance >= position.width)
                values2.RemoveAt(0);
            for (var i = 0; i < values2.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, ((1 - values2[i]) * (position.height / 2))), new Vector2(i * Distance + Distance, ((1 - values2[i + 1]) * (position.height / 2))));
                GUI.Label(new Rect(i * Distance, ((1 - values2[i]) * (position.height / 2)), 22.5f, 25), values2[i].ToString());
            }
        }

        if (values.Count > 1)
        {
            if (values.Count * Distance >= position.width)
                values.RemoveAt(0);

            for (var i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, (position.height / 2) + ((1 - values[i]) * (position.height / 2))), new Vector2(i * Distance + Distance, (position.height / 2) + ((1 - values[i + 1]) * (position.height / 2))));
                GUI.Label(new Rect(i * Distance, (position.height / 2) + ((1 - values[i]) * (position.height / 2)), 22.5f, 25), values[i].ToString());
            }
        }
    }

    private void TripleGraph()
    {
        if (values2.Count > 1)
        {
            if (values2.Count * Distance >= position.width)
                values2.RemoveAt(0);
            for (var i = 0; i < values2.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, ((1 - values2[i]) * (position.height / 3))), new Vector2(i * Distance + Distance, ((1 - values2[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * Distance, ((1 - values2[i]) * (position.height / 3)), 22.5f, 25), values2[i].ToString());
            }
        }

        if (values.Count > 1)
        {
            if (values.Count * Distance >= position.width)
                values.RemoveAt(0);

            for (var i = 0; i < values.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, (position.height / 3) + ((1 - values[i]) * (position.height / 3))), new Vector2(i * Distance + Distance, (position.height / 3) + ((1 - values[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * Distance, (position.height / 3) + ((1 - values[i]) * (position.height / 3)), 22.5f, 25), values[i].ToString());
            }
        }

        if (values3.Count > 1)
        {
            if (values3.Count * Distance >= position.width)
                values3.RemoveAt(0);

            for (var i = 0; i < values3.Count - 1; i++)
            {
                Handles.DrawLine(new Vector2(i * Distance, ((position.height / 3) * 2) + ((1 - values3[i]) * (position.height / 3))), new Vector2(i * Distance + Distance, ((position.height / 3) * 2) + ((1 - values3[i + 1]) * (position.height / 3))));
                GUI.Label(new Rect(i * Distance, ((position.height / 3) * 2) + ((1 - values3[i]) * (position.height / 3)), 22.5f, 25), values3[i].ToString());
            }
        }
    }

}
