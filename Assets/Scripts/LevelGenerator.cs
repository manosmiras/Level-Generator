using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class LevelGenerator : MonoBehaviour
{
    public static int overlapPenalty = 0;
    public static List<string> collisions = new List<string>();
    public static bool generated = false;
    public GameObject cross;
    public GameObject t_junction;
    public GameObject hall;
    public GameObject corner;
    public GameObject room;
    public GameObject aStar;
    List<DesignElement> population = new List<DesignElement>();

    float positionModifier = 2.5f;
    int populationSize = 50;
    // Use this for initialization
    void Start()
    {
        //if (GUILayout.Button("Generate"))
        //{
        GenerateRandomPopulation();
        DisplayPopulation();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Overlap penalty: " + overlapPenalty);
    }
    // Generates a random population
    public void GenerateRandomPopulation()
    {
        // Clear population
        population.Clear();
        for (int i = 0; i < populationSize; i++)
        {
            Vector2 position = new Vector2(Random.Range(-populationSize / 2, populationSize / 2) * positionModifier,
                                           Random.Range(-populationSize / 2, populationSize / 2) * positionModifier);
            float rotation = Random.Range(0, 2);
            rotation *= 90f;
            int roomType = Random.Range(0, 4);
            LevelPiece piece = new LevelPiece(position, rotation, (LevelPiece.Type)roomType);
            population.Add(piece);
        }
        Debug.Log("Population: " + population);
        Debug.Log(Quaternion.identity);
    }
    // Displays the population in Unity
    void DisplayPopulation()
    {
        //// Clear all children of current gameobject first
        //foreach (Transform child in gameObject.transform)
        //{
        //    Destroy(child.gameObject);
        //}
        int count = 0;
        foreach (LevelPiece piece in population)
        {
            count++;
            switch (piece.type)
            {
                case LevelPiece.Type.Cross:
                    GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCross.transform.parent = gameObject.transform;
                    tempCross.name += count;
                    break;
                case LevelPiece.Type.T_Junction:
                    GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempT_Junction.transform.parent = gameObject.transform;
                    break;
                case LevelPiece.Type.Hall:
                    GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempHall.transform.parent = gameObject.transform;
                    break;
                case LevelPiece.Type.Corner:
                    GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCorner.transform.parent = gameObject.transform;
                    break;
                case LevelPiece.Type.Room:
                    GameObject tempRoom = Instantiate(room, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempRoom.transform.parent = gameObject.transform;
                    break;
            }
        }
        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(), new Quaternion());
    }
    //void OnCollisionStay()
    //{
    //    print(gameObject.name + " collided with another object");
    //}

    void OnTriggerStay(Collider collisionInfo)
    {
        Debug.Log("Detected collision between " + gameObject.name + " and " + collisionInfo.name);
        //print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        //print("Their relative velocity is " + collisionInfo.relativeVelocity);
    }

}

// Creates a custom Label on the inspector for all the scripts named ScriptName
// Make sure you have a ScriptName script in your
// project, else this will not work.
[CustomEditor(typeof(LevelGenerator))]
public class TestOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //if (GUILayout.Button("Generate"))
        //{

        //}
    }
}