using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class LevelGenerator : MonoBehaviour
{
    public static int overlapPenalty = 0;
    public static int connectedCount = 0;
    public static int disconnectPenalty = 0;
    public static List<string> overlapping = new List<string>();
    public static List<string> connected = new List<string>();
    public static bool generated = false;
    public GameObject cross;
    public GameObject t_junction;
    public GameObject hall;
    public GameObject corner;
    public GameObject room;
    public GameObject aStar;
    private int currentIndividual = 0;
    private float cooldown = 0;
    private bool displaying = false;
    //List<DesignElement> individual = new List<DesignElement>();
    List<Individual> population = new List<Individual>();
    
    float positionModifier = 2.5f;
    public static int populationSize = 50;
    // Use this for initialization
    void Start()
    {
        //if (GUILayout.Button("Generate"))
        //{
        GenerateRandomPopulation(100);
        

        //}
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Overlap penalty: " + overlapPenalty);
        //Debug.Log("Disconnect penalty: " + (populationSize - connectedCount));
        DisplayPopulation();
    }
    // Generates a random population
    public void GenerateRandomPopulation(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GenerateRandomIndividual();
        }
    }
    // Generates a random individual
    public void GenerateRandomIndividual()
    {
        Individual individual = new Individual();
        
        // Clear population
        individual.designElements.Clear();
        for (int i = 0; i < populationSize; i++)
        {
            Vector2 position = new Vector2(Random.Range(-populationSize / 2, populationSize / 2) * positionModifier,
                                           Random.Range(-populationSize / 2, populationSize / 2) * positionModifier);
            float rotation = Random.Range(0, 2);
            rotation *= 90f;
            int roomType = Random.Range(0, 4);
            LevelPiece piece = new LevelPiece(position, rotation, (LevelPiece.Type)roomType);
            individual.designElements.Add(piece);
        }
        population.Add(individual);
        //Debug.Log("Population size: " + population.Count);
        //Debug.Log("Population: " + individual);
        //Debug.Log(Quaternion.identity);
    }
    // Displays the population in Unity
    void DisplayPopulation()
    {        
        if (!displaying && currentIndividual < population.Count)
        {
            // Reset variables
            overlapPenalty = 0;
            disconnectPenalty = 0;
            connectedCount = 0;
            overlapping.Clear();
            connected.Clear();
            // Clear scene before spawning level
            ClearScene();
            // Reset cooldown
            cooldown = 0;
            // Display the individual
            DisplayIndividual(population[currentIndividual]);

            displaying = true;

        }
        if (displaying)
        {
            cooldown += Time.deltaTime;
            if (cooldown >= .1)
            {
                // Calculate fitness for current individual
                population[currentIndividual].fitness = overlapPenalty + (populationSize - connectedCount);

                Debug.Log("I am individual number: " + currentIndividual + ", my OVERLAP penalty is: " + overlapPenalty +
                ", my DISCONNECT penalty is: " + (populationSize - connectedCount));

                currentIndividual++;
                displaying = false;
            }
        }
        //foreach (List<DesignElement> individual in population)
        //{
        //    DisplayIndividual(individual);
        //}

    }

    Individual Crossover(Individual individual1, Individual individual2)
    {
        Individual offspring = new Individual();
        // Loop through all design elements
        for (int i = 0; i < individual1.designElements.Count; i++)
        {
            float rand = Random.Range(0.0f, 1.0f);
            // 50% chance to copy design element from individual 1
            if (rand <= 0.5f)
            {
                offspring.designElements[i] = individual1.designElements[i];
            }
            // 50% chance to copy design element from individual 2
            else
            {
                offspring.designElements[i] = individual2.designElements[i];
            }
        }
        return offspring;
    }

    void DisplayIndividual(Individual individual)
    {
        int count = 0;
        foreach (LevelPiece piece in individual.designElements)
        {
            count++;
            switch (piece.type)
            {
                case LevelPiece.Type.Cross:
                    GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCross.transform.parent = gameObject.transform;
                    tempCross.name += count;
                    tempCross.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.T_Junction:
                    GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempT_Junction.transform.parent = gameObject.transform;
                    tempT_Junction.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Hall:
                    GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempHall.transform.parent = gameObject.transform;
                    tempHall.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Corner:
                    GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCorner.transform.parent = gameObject.transform;
                    tempCorner.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Room:
                    GameObject tempRoom = Instantiate(room, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempRoom.transform.parent = gameObject.transform;
                    tempRoom.tag = "LevelPiece";
                    break;
            }
        }
        // TODO: Fix overlap and disconnected penalties detection to work, maybe make coroutine or async?
        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(), new Quaternion());

        //currentIndividual++;
        //generated = false;
    }

    void ClearScene()
    {
        // Destroys A* prefab and any level pieces that were spawned
        Destroy(GameObject.FindGameObjectWithTag("A*"));
        GameObject[] levelPieces = GameObject.FindGameObjectsWithTag("LevelPiece");
        foreach (GameObject levelPiece in levelPieces)
        {
            DestroyImmediate(levelPiece, true);
        }
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