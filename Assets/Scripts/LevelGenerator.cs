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
    Fi2PopGA
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
    public GameObject cross;
    public GameObject t_junction;
    public GameObject hall;
    public GameObject corner;
    public GameObject room1;
    public GameObject room2;
    public GameObject room3;
    public GameObject aStar;
    public GameObject spikeTrap;
    public GameObject seekerFast;
    public GameObject seekerSafe;
    public GameObject seekerDangerous;
    public Technique technique;
    //public GameObject seekerSlow;
    public GameObject target;
    [ReadOnly] public int currentInfeasibleIndividual = 0;
    [ReadOnly] public int currentFeasibleIndividual = 0;
    [ReadOnly] public int generation = 1;
    [ReadOnly] public float fittestInfeasible = 0;
    [ReadOnly] public float fittestFeasible = 0;
    [ReadOnly] public float fitnessInfeasible = 0;
    [ReadOnly] public float fitnessFeasible = 0;
    [ReadOnly] public int overlap = 0;
    [ReadOnly] public int connection = 0;
    [ReadOnly] public int populationDiversity = 0;
    [ReadOnly] public int connectedComponents = 0;
    [ReadOnly] public bool pairVertexConnected = false;
    
    public int overlapPenaltyMultiplier = 1;
    public int connectionPenaltyMultiplier = 2;
    public int genomeLength = 10;
    public int populationSize = 500;
    public float positionModifier = 15f;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.05f;
    public bool elitism = true;
    public int tournamentSize;
    public float evaluationTime = 0.1f;
    public Population infeasiblePopulation = new Population();
    public Population feasiblePopulation = new Population();
    public Individual infeasibleFittest = new Individual();
    public Individual feasibleFittest = new Individual();
    public string time;
    public string timeMs;
    public int runtimeInSeconds = 300;
    [ReadOnly] public bool terminate = false;
    [ReadOnly] public bool initialisedInfeasiblePop;
    [ReadOnly] public bool initialisedFeasiblePop;
    public static Graph graph = new Graph();
    // Private properties
    private float cooldown = 0;
    private bool displaying = false;

    private static float uniformRate = 0.5f;
    
    private List<Vector3> trapPositions = new List<Vector3>();
    private bool finished = false;
    // Use this for initialization
    void Start()
    {
        initialisedInfeasiblePop = false;
        initialisedFeasiblePop = false;
        GenerateRandomPopulation(populationSize);

        generation = 1;
        fittestFeasible = 0;
        fittestInfeasible = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (technique)
        {
            case Technique.SimpleGA:
                SimpleGA();
                break;
            case Technique.Fi2PopGA:
                FI2PopGA();
                break;
            default:
                break;
        }
    }

    public void FI2PopGA()
    {
        if (Time.time <= runtimeInSeconds ^ terminate)//if (fittest != 1) //if (currentIndividual != 1)
        {
            timeMs = Time.time.ToString("F2");
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayInfeasiblePopulation(infeasiblePopulation);
            // Will spawn feasible levels and evaluate them
            if (feasiblePopulation.Size() >= 1 && initialisedInfeasiblePop)
            {
                
                Debug.Log("Trying to display feasible pop");
                
                DisplayFeasiblePopulation(feasiblePopulation);
            }
            //else
            //{
            //    initialisedFeasiblePop = true;
            //}
            if (initialisedInfeasiblePop && (initialisedFeasiblePop || feasiblePopulation.Size() == 0))
            {
                infeasiblePopulation.individuals.RemoveAll(x => x.delete == true);
                feasiblePopulation.individuals.RemoveAll(x => x.delete == true);

                generation++;
                // Evolve infeasible population
                infeasiblePopulation = EvolvePopulation(infeasiblePopulation);
                currentInfeasibleIndividual = 0;
                initialisedInfeasiblePop = false;
                // Evolve feasible population, if it exists
                if (initialisedFeasiblePop)
                {
                    feasiblePopulation = EvolvePopulation(feasiblePopulation);
                    currentFeasibleIndividual = 0;
                    initialisedFeasiblePop = false;
                }
            }
        }
        else if (!finished)
        {
            //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");

            Debug.Log("Clearing and spawning fittest, it has a fitness of: " + infeasiblePopulation.GetFittest().fitness);
            ClearScene();

            // Show fittest individual
            //DisplayIndividual(final);
            if (feasiblePopulation.Size() > 0)
                DisplayIndividual(feasiblePopulation.GetFittest());
            else
                DisplayIndividual(infeasiblePopulation.GetFittest());
            finished = true;
        }
    }

    public void SimpleGA()
    {
        if (Time.time <= runtimeInSeconds ^ terminate)//if (fittest != 1) //if (currentIndividual != 1)
        {
            timeMs = Time.time.ToString("F2");
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayInfeasiblePopulation(infeasiblePopulation);

            if (initialisedInfeasiblePop)
            {

                generation++;
                //Debug.Log("Before: " + fittestIndividual.fitness + " - " + population.GetFittest().fitness + ", " + generation);
                //System.out.println("Generation: " + generationCount + " Fittest: " + myPop.getFittest().getFitness());

                infeasiblePopulation = EvolvePopulation(infeasiblePopulation);
                //Debug.Log("After: " + fittestIndividual.fitness + " - " + population.GetFittest().fitness + ", " + generation);
                currentInfeasibleIndividual = 0;
                initialisedInfeasiblePop = false;

            }
        }
        else if (!finished)
        {
            //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");

            Debug.Log("Clearing and spawning fittest, it has a fitness of: " + infeasiblePopulation.GetFittest());
            ClearScene();

            // Show fittest individual
            //DisplayIndividual(final);
            DisplayIndividual(infeasiblePopulation.GetFittest());
            finished = true;
        }
    }

    Population EvolvePopulation(Population pop)
    {
        Population newPopulation = new Population();

        // Keep our best individual
        if (elitism)
        {
            // Add fittest to new population, will not be affected by crossover or mutation
            newPopulation.Add(Utility.DeepClone(infeasiblePopulation.GetFittest()));
            // Add fittest to new population a second time, will be affected by mutation
            newPopulation.Add(Utility.DeepClone(infeasiblePopulation.GetFittest()));

        }

        int elitismOffset;
        if (elitism)
        {
            elitismOffset = 1;
        }
        else
        {
            elitismOffset = 0;
        }

        // Loop over the population size and create new individuals with
        // crossover
        for (int i = elitismOffset + 1; i < pop.Size(); i++)
        {
            Individual individual1 = TournamentSelection(pop);
            Individual individual2 = TournamentSelection(pop);
            Individual newIndividual = new Individual();
            // Perform uniform crossover every 5 generations, for the rest use single point crossover
            if (generation % 5 == 0)
                newIndividual = UniformCrossover(individual1, individual2);
            else
                newIndividual = SinglePointCrossover(individual1, individual2);

            newPopulation.Add(newIndividual);
        }

        // Mutate population
        for (int i = elitismOffset; i < pop.Size(); i++)
        {
            // Always mutate 
            if (i == elitismOffset)
            {
                Mutate(newPopulation.individuals[i]);
            }
            // mutationRate % chance of mutating
            if (Random.Range(0.0f, 1.0f) <= mutationRate)
            {
                Mutate(newPopulation.individuals[i]);
            }
        }

        //newPopulation.individuals[0] = Individual.FromJson(Application.dataPath + "/" + "gl" + genomeLength + "f" + fittest + ".json");
        //newPopulation.individuals[0] = Utility.DeepClone(fittestIndividual);
        //pop = newPopulation;
        return newPopulation;
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

        // Clear
        individual.designElements.Clear();
        for (int i = 0; i < genomeLength; i++)
        {
            float rotation = Random.Range(0, 4);
            rotation *= 90f;
            int roomType = Random.Range(0, 7);
            LevelPiece piece = new LevelPiece(Vector2.zero, rotation, (LevelPiece.Type)roomType);
            individual.designElements.Add(piece);

        }
        infeasiblePopulation.Add(individual);
    }
    // Displays the infeasible population in Unity
    void DisplayInfeasiblePopulation(Population pop)
    {
        if (!displaying && currentInfeasibleIndividual < pop.Size())
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
            DisplayIndividual(pop.individuals[currentInfeasibleIndividual]);

            displaying = true;

        }
        if (displaying && !(currentInfeasibleIndividual >= pop.Size()))
        {
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime)
            {

                connectedComponents = graph.CalculateConnectivity();
                // Calculates the 2-vertex-connectivity of the graph
                //int kConnectivity = graph.CalculateKConnectivity(2);
                //float kConnectivity = graph.CalculateVariableKConnectivity();

                GraphEditor.InitRects(genomeLength);

                switch (technique)
                {
                    case Technique.SimpleGA:
                        int kConnectivity = graph.CalculateVariableKConnectivity();
                        pop.individuals[currentInfeasibleIndividual].fitness = (genomeLength - connectedComponents) + kConnectivity;
                        break;
                    case Technique.Fi2PopGA:
                        pop.individuals[currentInfeasibleIndividual].fitness = (genomeLength - connectedComponents);
                        break;
                    default:
                        break;
                }

                // Feasible
                if (connectedComponents == 1)
                {
                    Debug.Log("Found a graph with 1 connected component.");
                    //fitness += genomeLength / 2;
                    // Add to feasible population only if solution is different
                    if (!feasiblePopulation.individuals.Contains(pop.individuals[currentInfeasibleIndividual]))
                    {
                        Individual feasibleIndividual = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);
                        feasiblePopulation.Add(feasibleIndividual);
                        pop.individuals[currentInfeasibleIndividual].delete = true;
                        //feasibleIndividual.ToJson("feasible" + feasiblePopulation.individuals.Count);
                    }
                }

                fitnessInfeasible = pop.individuals[currentInfeasibleIndividual].fitness;
                
                if (fitnessInfeasible >= fittestInfeasible)// && !pop.individuals[currentIndividual].Equals(fittestIndividual))
                {
                    fittestInfeasible = fitnessInfeasible;
                    infeasibleFittest = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);
                    //Debug.Log("Updated fittest individual at generation: " + generation);
                    //fittestIndividual.ToJson();
//                    LevelGeneratorEditor.load = true;
//                    ScreenCapture.CaptureScreenshot("Assets/Resources/FittestLevel.png");
//#if UNITY_EDITOR
//                    AssetDatabase.Refresh();
//#endif
                }

                currentInfeasibleIndividual++;
                if (currentInfeasibleIndividual == infeasiblePopulation.Size())
                {
                    initialisedInfeasiblePop = true;
                }
                displaying = false;
            }
        }

    }

    // Displays the feasible population in Unity
    void DisplayFeasiblePopulation(Population pop)
    {
        
        if (!displaying && currentFeasibleIndividual < pop.Size())
        {
            // Reset variables
            connectedCount = 0;
            overlapping.Clear();
            connected.Clear();
            // Clear scene before spawning level
            ClearScene();
            // Reset cooldown
            cooldown = 0;
            // Display the individual
            DisplayIndividual(pop.individuals[currentFeasibleIndividual]);

            displaying = true;
            
        }
        if (displaying && !(currentFeasibleIndividual >= pop.Size()))
        {
            Debug.Log("before cooldown");
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime)
            {
                Debug.Log("went into cooldown");
                connectedComponents = graph.CalculateConnectivity();
                // Calculates the 2-vertex-connectivity of the graph
                //int kConnectivity = graph.CalculateKConnectivity(2);
                int kConnectivity = graph.CalculateVariableKConnectivity();

                GraphEditor.InitRects(genomeLength);

                pop.individuals[currentFeasibleIndividual].fitness = (genomeLength - connectedComponents) + kConnectivity;
                //pop.individuals[currentFeasibleIndividual].objectiveFitness = kConnectivity;
                // Infeasible
                if (connectedComponents != 1)
                {
                    pop.individuals[currentFeasibleIndividual].fitness = (genomeLength - connectedComponents);
                    // Move to infeasible population
                    //infeasiblePopulation.individuals[infeasiblePopulation.GetWeakestIndex()] = Utility.DeepClone(pop.individuals[currentFeasibleIndividual]);
                    infeasiblePopulation.Add(Utility.DeepClone(pop.individuals[currentFeasibleIndividual]));
                    pop.individuals[currentFeasibleIndividual].delete = true;
                    Debug.Log("Adding from feasible to infeasible, size becomes: " + infeasiblePopulation.Size());
                }

                fitnessFeasible = pop.individuals[currentFeasibleIndividual].fitness;

                if (fitnessFeasible >= fittestFeasible)
                {
                    fittestFeasible = fitnessFeasible;
                    feasibleFittest = Utility.DeepClone(pop.individuals[currentFeasibleIndividual]);
                }

                currentFeasibleIndividual++;
                if (currentFeasibleIndividual == feasiblePopulation.Size())
                {
                    initialisedFeasiblePop = true;
                }
                displaying = false;
            }
        }

    }

    Individual UniformCrossover(Individual individual1, Individual individual2)
    {
        Individual offspring = new Individual();
        // Loop through all design elements
        for (int i = 0; i < individual1.designElements.Count; i++)
        {
            if (individual1.fitness > individual2.fitness)
            {
                float rand = Random.Range(0.0f, 1.0f);
                // 60% chance to copy design element from individual 1
                if (rand <= uniformRate)
                {
                    offspring.designElements.Add(individual1.designElements[i]);
                }
                // 40% chance to copy design element from individual 2
                else
                {
                    offspring.designElements.Add(individual2.designElements[i]);
                }
            }
            else
            {
                float rand = Random.Range(0.0f, 1.0f);
                // 60% chance to copy design element from individual 2
                if (rand <= uniformRate)
                {
                    offspring.designElements.Add(individual2.designElements[i]);
                }
                // 40% chance to copy design element from individual 1
                else
                {
                    offspring.designElements.Add(individual1.designElements[i]);
                }
            }
        }
        return offspring;
    }

    Individual SinglePointCrossover(Individual individual1, Individual individual2)
    {
        Individual offspring = new Individual();
        int point = genomeLength / 2;
        float rand = Random.Range(0.0f, 1.0f);
        // Loop through all design elements
        for (int i = 0; i < individual1.designElements.Count; i++)
        {
            if (rand <= 0.5)
            {
                // Copies half of individual 1 and half of individual 2
                if (i <= point)
                {
                    offspring.designElements.Add(individual1.designElements[i]);
                }
                else
                {
                    offspring.designElements.Add(individual2.designElements[i]);
                }
            }
            else
            {
                // Copies half of individual 1 and half of individual 2
                if (i <= point)
                {
                    offspring.designElements.Add(individual2.designElements[i]);
                }
                else
                {
                    offspring.designElements.Add(individual1.designElements[i]);
                }
            }
        }
        return offspring;
    }

    Individual TournamentSelection(Population pop)
    {
        // Create a tournament population
        Population tournamentPopulation = new Population();

        // For each place in the tournament get a random individual
        for (int i = 0; i < tournamentSize; i++)
        {
            int randomId = Random.Range(0, pop.Size());
            tournamentPopulation.Add(Utility.DeepClone(pop.individuals[randomId]));
        }
        // Get the fittest
        Individual fittest = tournamentPopulation.GetFittest();

        return fittest;
    }

    // Mutate an individual
    void Mutate(Individual individual)
    {
        // Loop through genes
        for (int i = 0; i < individual.designElements.Count; i++)
        {

            int mutationType = Random.Range(0, 2);
            if (mutationType == 0)
            {
                MutateRotation(individual.designElements[i]);
            }
            else if (mutationType == 1)
            {
                MutateLevelPiece((LevelPiece)individual.designElements[i]);
            }

        }
    }

    void MutatePosition(DesignElement designElement)
    {
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
        float yMax = Mathf.CeilToInt(genomeLength / xMax);

        Vector2 position = new Vector2(Random.Range(0, (int)xMax) * positionModifier,
                           Random.Range(0, (int)yMax) * positionModifier);

        // Keep re-positioning until value is different
        while (position == designElement.position)
        {
            position = new Vector2(Random.Range(0, (int)xMax) * positionModifier,
                               Random.Range(0, (int)yMax) * positionModifier);
        }
        designElement.position = position;
    }

    void MutateRotation(DesignElement designElement)
    {
        // Select a random rotation between 0, 90, 180, 270 degrees
        float rotation = Random.Range(0, 4);
        // Keep rotating until value is different
        while (rotation * 90f == designElement.rotation)
        {
            rotation = Random.Range(0, 4);
        }
        rotation *= 90f;
        designElement.rotation = rotation;
    }

    void MutateLevelPiece(LevelPiece levelPiece)
    {
        // Select random room type
        int roomType = Random.Range(0, 7);
        // Keep chaning piece until it's different
        while ((LevelPiece.Type)roomType == levelPiece.type)
        {
            roomType = Random.Range(0, 7);
        }
        levelPiece.type = (LevelPiece.Type)roomType;
    }

    void DisplayIndividual(Individual individual)
    {
        int count = 0;
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
        float yMax = Mathf.CeilToInt(genomeLength / xMax);
        // Initialise graph
        graph.nodes = new List<GraphNode>();
        // Initialise trap positions list
        trapPositions = new List<Vector3>();
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
                    }
                }
                else
                {
                    break;
                }
                count++;
            }
        }
        trapPositions = CollectPossibleTrapPositions();
        // TEST
        for (int i = 0; i < genomeLength; i++)
        {
            // Choose a position randomly
            int randPos = Random.Range(0, trapPositions.Count);
            // Spawn trap
            GameObject tempSpikeTrap = Instantiate(spikeTrap, new Vector3(trapPositions[randPos].x, trapPositions[randPos].y - 2.5f, trapPositions[randPos].z), Quaternion.AngleAxis(0, Vector3.up)) as GameObject;
            tempSpikeTrap.transform.parent = gameObject.transform;
            tempSpikeTrap.name += i;
            // Remove currently selected position
            trapPositions.Remove(trapPositions[randPos]);
        }

        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(), new Quaternion());
        float distance = 0;
        Vector3 furthest = new Vector3();
        for (int i = 1; i < piecePositions.Count; i++)
        {
            float currentDistance = ManhattanDistance(piecePositions[0], piecePositions[i]);
            if (currentDistance > distance)
            {
                furthest = piecePositions[i];
                distance = currentDistance;
            }
        }

        Instantiate(target, furthest, new Quaternion());
        Instantiate(seekerFast, piecePositions[0], new Quaternion());
        Instantiate(seekerSafe, piecePositions[0], new Quaternion());
        Instantiate(seekerDangerous, piecePositions[0], new Quaternion());
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

        List<GameObject> levelPieces = new List<GameObject>(corners);
        levelPieces.AddRange(crosses);
        levelPieces.AddRange(halls);
        levelPieces.AddRange(t_junctions);
        levelPieces.AddRange(rooms1);
        levelPieces.AddRange(rooms2);
        levelPieces.AddRange(rooms3);

        return levelPieces;
    }

    List<GameObject> CollectTraps()
    {
        GameObject[] spikeTraps = GameObject.FindGameObjectsWithTag("SpikeTrap");

        List<GameObject> traps = new List<GameObject>(spikeTraps);

        return traps;
    }
    List<Vector3> CollectPossibleTrapPositions()
    {
        GameObject[] possiblePositions = GameObject.FindGameObjectsWithTag("TrapPosition");

        List<Vector3> positions = new List<Vector3>();

        foreach (GameObject go in possiblePositions)
        {
            positions.Add(go.transform.position);
        }

        return positions;
    }

    void ClearScene()
    {
        // Destroys A* prefab
        Destroy(GameObject.FindGameObjectWithTag("A*"));
        // Destroys Target prefab
        Destroy(GameObject.FindGameObjectWithTag("Target"));
        // Destroy any level pieces that were spawned
        List<GameObject> levelPieces = CollectLevelPieces();
        foreach (GameObject levelPiece in levelPieces)
        {
            DestroyImmediate(levelPiece, true);
        }
        // Destroy any Traps that were spawned
        List<GameObject> traps = CollectTraps();
        
        foreach (GameObject trap in traps)
        {
            DestroyImmediate(trap, true);
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