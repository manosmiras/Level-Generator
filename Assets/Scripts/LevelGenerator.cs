// LevelGenerator.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEditor;
public class ReadOnlyAttribute : PropertyAttribute { }

public enum Technique
{
    SimpleGA,
    FI2PopGA,
    NoveltySearchGA,
    FI2PopNsGA
}

public class LevelGenerator : MonoBehaviour
{
    // Public properties
    public static int overlapPenalty = 0;
    public static int connectedCount = 0;
    public static int disconnectPenalty = 0;
    public static List<string> overlapping = new List<string>();
    public static List<string> connected = new List<string>();
    public static bool generated = false;
    // Rooms
    public GameObject cross;
    public GameObject t_junction;
    public GameObject hall;
    public GameObject corner;
    public GameObject room1;
    public GameObject room2;
    public GameObject room3;
    public GameObject room4;
    // Trap rooms
    public GameObject crossTrap;
    public GameObject t_junctionTrap;
    public GameObject hallTrap;
    public GameObject cornerTrap;
    public GameObject room1Trap;
    public GameObject room2Trap;
    public GameObject room3Trap;
    public GameObject room4Trap;

    public GameObject aStar;
    public GameObject spikeTrap;
    public GameObject seekerFast;
    public GameObject seekerSafe;
    public GameObject seekerDangerous;
    //public GameObject seekerSlow;
    public GameObject target;
    public Technique technique;
    public bool minimalCriteria = true;
    public bool deathPenalty = false;
    public GeneticAlgorithm.CrossoverType crossoverType;
    public bool elitism = true;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.05f;
    public bool displayCeiling = true;
    public int genomeLength = 10;
    public int populationSize = 50;
    public int tournamentSize;
    public float evaluationTime = 0.1f;
    public int maxGeneration = 20;
    public float positionModifier = 15f;
    public bool testing;
    public string testName;
    public int testRuns = 10;
    //[ReadOnly] public int currentInfeasibleIndividual = 0;
    //[ReadOnly] public int currentFeasibleIndividual = 0;
    //[ReadOnly] public int currentIndividual = 0;
    [ReadOnly] public int generation = 1;

    public string time;

    [ReadOnly] public bool terminate = false;

    public static int shortestPathCost = 0;

    public static Graph graph = new Graph();

    // Private properties
    private bool init = true;
    private SimpleGA simpleGA;
    private FI2PopGA fi2PopGA;
    private NoveltySearchGA noveltySearchGA;
    private FI2PopNsGA fi2PopNsGA;
    // Use this for initialization 
    void Start()
    {
        FitnessVisualizerEditor.technique = technique;
        simpleGA = new SimpleGA(populationSize, genomeLength, mutationRate, elitism, crossoverType, tournamentSize, evaluationTime, testing, testRuns, maxGeneration, this);
        fi2PopGA = new FI2PopGA(populationSize, genomeLength, mutationRate, elitism, crossoverType, tournamentSize, evaluationTime, testing, testRuns, maxGeneration, this);
        noveltySearchGA = new NoveltySearchGA(minimalCriteria, deathPenalty, populationSize, genomeLength, mutationRate, elitism, crossoverType, tournamentSize, evaluationTime, testing, testRuns, maxGeneration, this);
        fi2PopNsGA = new FI2PopNsGA(minimalCriteria, deathPenalty, populationSize, genomeLength, mutationRate, elitism, crossoverType, tournamentSize, evaluationTime, testing, testRuns, maxGeneration, this);
    }

    // Update is called once per frame
    void Update()
    {
        switch (technique)
        {
            case Technique.SimpleGA:
                simpleGA.Run();
                simpleGA.csvFileName = testName;
                generation = simpleGA.generation;
                break;
            case Technique.FI2PopGA:
                fi2PopGA.Run();
                fi2PopGA.csvFileName = testName;
                generation = fi2PopGA.generation;
                break;
            case Technique.NoveltySearchGA:
                noveltySearchGA.Run();
                noveltySearchGA.csvFileName = testName;
                generation = noveltySearchGA.generation;
                break;
            case Technique.FI2PopNsGA:
                fi2PopNsGA.Run();
                fi2PopNsGA.csvFileName = testName;
                generation = fi2PopNsGA.generation;
                break;
            default:
                break;
        }
        
    }

    public void DisplayIndividual(Individual individual)
    {
        int genomeLength = individual.designElements.Count;
        int count = 0;
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
        float yMax = Mathf.CeilToInt(genomeLength / xMax);
        // Initialise graph
        graph.nodes = new List<GraphNode>();

        List<Vector3> piecePositions = new List<Vector3>();
        for (int x = 0; x < (int)xMax; x++)
        {
            for (int y = 0; y < (int)yMax; y++)
            {
                //Debug.Log(new Vector2(x, y));
                if (count < genomeLength)
                {

                    LevelPiece piece = (LevelPiece)individual.designElements[count];
                    piece.position.x = (x * positionModifier) - (xMax * positionModifier) / 2;
                    piece.position.y = (y * positionModifier) - (yMax * positionModifier) / 2;
                    piecePositions.Add(new Vector3(piece.position.x, 0, piece.position.y));
                    switch (piece.type)
                    {
                        // Rooms
                        case LevelPiece.Type.Cross:
                            GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCross.transform.parent = gameObject.transform;
                            tempCross.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempCross.name, count));
                            break;
                        case LevelPiece.Type.T_Junction:
                            GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempT_Junction.transform.parent = gameObject.transform;
                            tempT_Junction.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempT_Junction.name, count));
                            break;
                        case LevelPiece.Type.Hall:
                            GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempHall.transform.parent = gameObject.transform;
                            tempHall.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempHall.name, count));
                            break;
                        case LevelPiece.Type.Corner:
                            GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCorner.transform.parent = gameObject.transform;
                            tempCorner.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempCorner.name, count));
                            break;
                        case LevelPiece.Type.Room1:
                            GameObject tempRoom1 = Instantiate(room1, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom1.transform.parent = gameObject.transform;
                            tempRoom1.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom1.name, count));
                            break;
                        case LevelPiece.Type.Room2:
                            GameObject tempRoom2 = Instantiate(room2, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom2.transform.parent = gameObject.transform;
                            tempRoom2.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom2.name, count));
                            break;
                        case LevelPiece.Type.Room3:
                            GameObject tempRoom3 = Instantiate(room3, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom3.transform.parent = gameObject.transform;
                            tempRoom3.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom3.name, count));
                            break;
                        case LevelPiece.Type.Room4:
                            GameObject tempRoom4 = Instantiate(room4, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom4.transform.parent = gameObject.transform;
                            tempRoom4.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom4.name, count));
                            break;
                        // Trap rooms
                        case LevelPiece.Type.Cross_Trap:
                            GameObject tempCrossTrap = Instantiate(crossTrap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCrossTrap.transform.parent = gameObject.transform;
                            tempCrossTrap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempCrossTrap.name, count));
                            break;
                        case LevelPiece.Type.T_Junction_Trap:
                            GameObject tempT_JunctionTrap = Instantiate(t_junctionTrap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempT_JunctionTrap.transform.parent = gameObject.transform;
                            tempT_JunctionTrap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempT_JunctionTrap.name, count));
                            break;
                        case LevelPiece.Type.Hall_Trap:
                            GameObject tempHallTrap = Instantiate(hallTrap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempHallTrap.transform.parent = gameObject.transform;
                            tempHallTrap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempHallTrap.name, count));
                            break;
                        case LevelPiece.Type.Corner_Trap:
                            GameObject tempCornerTrap = Instantiate(cornerTrap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCornerTrap.transform.parent = gameObject.transform;
                            tempCornerTrap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempCornerTrap.name, count));
                            break;
                        case LevelPiece.Type.Room1_Trap:
                            GameObject tempRoom1Trap = Instantiate(room1Trap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom1Trap.transform.parent = gameObject.transform;
                            tempRoom1Trap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom1Trap.name, count));
                            break;
                        case LevelPiece.Type.Room2_Trap:
                            GameObject tempRoom2Trap = Instantiate(room2Trap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom2Trap.transform.parent = gameObject.transform;
                            tempRoom2Trap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom2Trap.name, count));
                            break;
                        case LevelPiece.Type.Room3_Trap:
                            GameObject tempRoom3Trap = Instantiate(room3Trap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom3Trap.transform.parent = gameObject.transform;
                            tempRoom3Trap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom3Trap.name, count));
                            break;
                        case LevelPiece.Type.Room4_Trap:
                            GameObject tempRoom4Trap = Instantiate(room4Trap, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom4Trap.transform.parent = gameObject.transform;
                            tempRoom4Trap.name += count;
                            // Add a new node to graph
                            graph.nodes.Add(new GraphNode(tempRoom4Trap.name, count));
                            break;
                    }
                }
                else
                {
                    break;
                }
                count++;
            }
        }

        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(piecePositions[0].x - 7.5f, 0f, piecePositions[0].z - 7.5f), new Quaternion());
        float distance = 0;
        Vector3 furthest = new Vector3();
        for (int i = 0; i < piecePositions.Count; i++)
        {
            float currentDistance = ManhattanDistance(piecePositions[0], piecePositions[i]);
            if (currentDistance > distance)
            {
                furthest = piecePositions[i];
                distance = currentDistance;
            }
        }
        HideCeiling(!displayCeiling);
        if (init)
        {
            Instantiate(target, furthest, new Quaternion());
            init = false;
        }

        //Instantiate(seekerFast, piecePositions[0], new Quaternion());
        Instantiate(seekerSafe, piecePositions[0], new Quaternion());
        //Instantiate(seekerDangerous, piecePositions[0], new Quaternion());
        //Instantiate(seekerSlow, piecePositions[0], new Quaternion());
    }

    List<GameObject> CollectLevelPieces()
    {
        GameObject[] corners = GameObject.FindGameObjectsWithTag("Corner");
        GameObject[] crosses = GameObject.FindGameObjectsWithTag("Cross");
        GameObject[] halls = GameObject.FindGameObjectsWithTag("Hall");
        GameObject[] t_junctions = GameObject.FindGameObjectsWithTag("T_Junction");
        GameObject[] rooms1 = GameObject.FindGameObjectsWithTag("Room1");
        GameObject[] rooms2 = GameObject.FindGameObjectsWithTag("Room2");
        GameObject[] rooms3 = GameObject.FindGameObjectsWithTag("Room3");
        GameObject[] rooms4 = GameObject.FindGameObjectsWithTag("Room4");

        List<GameObject> levelPieces = new List<GameObject>(corners);
        levelPieces.AddRange(crosses);
        levelPieces.AddRange(halls);
        levelPieces.AddRange(t_junctions);
        levelPieces.AddRange(rooms1);
        levelPieces.AddRange(rooms2);
        levelPieces.AddRange(rooms3);
        levelPieces.AddRange(rooms4);

        return levelPieces;
    }

    public void InvokeSpawnWallsOnDeadEnds(float evaluationTime)
    {
        Invoke("SpawnWallsOnDeadEnds", evaluationTime);
    }

    // Spawn walls on dead ends
    public void SpawnWallsOnDeadEnds()
    {
        GameObject[] entryColliders = GameObject.FindGameObjectsWithTag("Entry_Colliders");
        foreach (GameObject entryCollider in entryColliders)
        {
            DisconnectChecker dc = entryCollider.GetComponent<DisconnectChecker>();
            //Debug.Log(dc.didCollide);
            if (!dc.didCollide)
            {
                Transform[] children = entryCollider.GetComponentsInChildren<Transform>(true);
                //Debug.Log(children.Length);
                foreach (Transform child in children)
                {
                    if (child.name.Contains("Wall with Collider"))
                    {
                        //Debug.Log("Setting wall active");
                        child.gameObject.SetActive(true);
                    }
                }

            }
        }
    }

    void HideCeiling(bool hide)
    {
        GameObject[] ceilings = GameObject.FindGameObjectsWithTag("Ceiling");
        foreach (GameObject ceiling in ceilings)
        {
            ceiling.SetActive(!hide);
        }
    }

    public void ClearScene()
    {
        // Destroys A* prefab
        Destroy(GameObject.FindGameObjectWithTag("A*"));
        // Destroys Target prefab
        //Destroy(GameObject.FindGameObjectWithTag("Target"));
        // Destroy any level pieces that were spawned
        List<GameObject> levelPieces = CollectLevelPieces();
        foreach (GameObject levelPiece in levelPieces)
        {
            DestroyImmediate(levelPiece, true);
        }

        GameObject[] seekers = GameObject.FindGameObjectsWithTag("Seeker");
        // Destroys Seeker prefab
        foreach (GameObject seeker in seekers)
        {
            DestroyImmediate(seeker, true);
        }
    }

    public float Area(GameObject gameObject)
    {
        //Transform transform = gameObject.transform.Find(gameObject.name.Substring(0, gameObject.name.Length - 8));
        //Debug.Log("looking for: " + gameObject.name.Substring(0, gameObject.name.Length - 8));
        MeshFilter[] meshFilters = transform.gameObject.GetComponentsInChildren<MeshFilter>();

        Vector3 result = Vector3.zero;
        foreach (MeshFilter mf in meshFilters)
        {
            Mesh mesh = mf.mesh;
            Vector3[] vertices = mesh.vertices;


            for (int p = vertices.Length - 1, q = 0; q < vertices.Length; p = q++)
            {
                result += Vector3.Cross(vertices[q], vertices[p]);
            }
            result *= 0.5f;

        }
        Debug.Log("Area of " + gameObject.tag + ": " + result.magnitude);
        return result.magnitude;
    }

    public int CalculateMaxConnections(Individual individual)
    {
        GameObject[] corners = GameObject.FindGameObjectsWithTag("Corner");
        GameObject[] crosses = GameObject.FindGameObjectsWithTag("Cross");
        GameObject[] halls = GameObject.FindGameObjectsWithTag("Hall");
        GameObject[] t_junctions = GameObject.FindGameObjectsWithTag("T_Junction");
        GameObject[] rooms1 = GameObject.FindGameObjectsWithTag("Room1");
        GameObject[] rooms2 = GameObject.FindGameObjectsWithTag("Room2");
        GameObject[] rooms3 = GameObject.FindGameObjectsWithTag("Room3");

        int maxConnections = corners.Length * 2 + crosses.Length * 4 + halls.Length * 2 +
            t_junctions.Length * 3 + rooms1.Length + rooms2.Length * 2 + rooms3.Length * 3;

        return maxConnections;
    }

    public float ManhattanDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
    }

}