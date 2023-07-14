using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
public class GraphEditor : EditorWindow
{
    private static List<Rect> _rects = new();
    private static bool _connect;
    
    [MenuItem("Window/Graph Editor Window")]
    private static void Init()
    {
        GetWindow(typeof(GraphEditor));
    }

    private void OnGUI()
    {
        if (_connect)
        {
            DrawConnections();
        }

        BeginWindows();

        for (var i = 0; i < _rects.Count; i++)
        {
            _rects[i] = GUI.Window(i, _rects[i], WindowFunction, i.ToString());
        }

        EndWindows();
        Repaint();
    }

    public static void InitRects(int size)
    {
        
        _rects = new List<Rect>();
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(size));
        float yMax = Mathf.CeilToInt(size / xMax);

        var count = 0;
        for (var x = 0; x < (int)xMax; x++)
        {
            for (var y = 0; y < (int)yMax; y++)
            {
                if (count < size)
                {
                    var rect = new Rect(10 + y * 100, 10 + x * 100, 50, 50);
                    _rects.Add(rect);
                }
                else
                {
                    break;
                }
                count++;
            }
        }
        _connect = true;
    }

    private static void DrawConnections()
    {
        foreach (var parent in LevelGenerator.graph.nodes)
        {
            foreach (var child in parent.children)
            {
                DrawConnection(_rects[parent.id], _rects[child.id]);
            }
        }
        
    }

    private static void DrawConnection(Rect windowRect, Rect windowRect2)
    {
        Handles.BeginGUI();
        Handles.color = Color.black;
        Handles.DrawLine(windowRect.center, windowRect2.center);
        Handles.EndGUI();
    }

    private void WindowFunction(int windowID)
    {
        GUI.DragWindow();
    }
}