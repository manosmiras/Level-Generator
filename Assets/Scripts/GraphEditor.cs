using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
public class GraphEditor : EditorWindow
{
    //Rect windowRect = new Rect(100 + 100, 100, 50, 50);
    //Rect windowRect2 = new Rect(100, 100, 50, 50);
    static List<Rect> rects = new List<Rect>();
    static bool connect = false;
    [MenuItem("Window/Graph Editor Window")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(GraphEditor));
    }

    private void OnGUI()
    {
        if (connect)
        {
            DrawConnections();
        }

        BeginWindows();

        for (int i = 0; i < rects.Count; i++)
        {
            rects[i] = GUI.Window(i, rects[i], WindowFunction, i.ToString());
        }

        EndWindows();
        Repaint();
    }

    public static void InitRects(int size)
    {
        
        rects = new List<Rect>();
        //for (int i = 0; i < size; i++)
        //{
        //    Rect rect = new Rect(10 + i * 100, 100, 50, 50);
        //    rects.Add(rect);
        //}
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(size));
        float yMax = Mathf.CeilToInt(size / xMax);

        int count = 0;
        for (int x = 0; x < (int)xMax; x++)
        {
            for (int y = 0; y < (int)yMax; y++)
            {
                if (count < size)
                {
                    Rect rect = new Rect(10 + y * 100, 10 + x * 100, 50, 50);
                    rects.Add(rect);
                }
                else
                {
                    break;
                }
                count++;
            }
        }
        connect = true;
    }

    public static void DrawConnections()
    {
        foreach (GraphNode parent in LevelGenerator.graph.nodes)
        {
            foreach (GraphNode child in parent.children)
            {
                DrawConnection(rects[parent.id], rects[child.id]);
            }
        }
        
    }

    public static void DrawConnection(Rect windowRect, Rect windowRect2)
    {
        Handles.BeginGUI();
        //Handles.DrawBezier(windowRect.center, windowRect2.center, new Vector2(windowRect.xMax + 50f, windowRect.center.y), new Vector2(windowRect2.xMin - 50f, windowRect2.center.y), Color.black, null, 3f);
        Handles.color = Color.black;
        Handles.DrawLine(windowRect.center, windowRect2.center);
        Handles.EndGUI();
    }

    void WindowFunction(int windowID)
    {
        GUI.DragWindow();
    }
}