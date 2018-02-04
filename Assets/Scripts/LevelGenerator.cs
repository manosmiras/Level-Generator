// LevelGenerator.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
public class ReadOnlyAttribute : PropertyAttribute { }

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
    [ReadOnly] public int currentIndividual = 0;
    [ReadOnly] public int generation = 1;
    [ReadOnly] public int fittest;
    [ReadOnly] public int fitness = 0;
    [ReadOnly] public int overlap = 0;
    [ReadOnly] public int connection = 0;
    [ReadOnly] public int populationDiversity = 0;
    [ReadOnly] public float populationArea = 0;
    [ReadOnly] public int connectedComponents = 0;
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
    public Population population = new Population();
    public Individual fittestIndividual = new Individual();
    public string time;
    
    [ReadOnly] public bool initialised;
    // Private properties
    private float cooldown = 0;
    private bool displaying = false;

    private static float uniformRate = 0.5f;

    private List<Vector3> trapPositions = new List<Vector3>();

    // Use this for initialization
    void Start()
    {
        initialised = false;
        GenerateRandomPopulation(populationSize);

        generation = 1;
        fittest = int.MaxValue;
        //elitism = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (fittest != 1) // if (currentIndividual != 3) //
        {
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);
            DisplayPopulation();

            //if (initialised)
            //{
            //    EvolvePopulation();
            //}
            if (initialised)
            {
                Debug.Log("fitnesses: " + fittestIndividual.fitness + ", " + population.GetFittest().fitness);
                //population.Sort();
                //population.Print();

                generation++;
                //System.out.println("Generation: " + generationCount + " Fittest: " + myPop.getFittest().getFitness());
                population = EvolvePopulation(population);

                currentIndividual = 0;
                initialised = false;

            }
        }

    }

    Population EvolvePopulation(Population pop)
    {
        Population newPopulation = new Population();

        // Keep our best individual
        if (elitism)
        {

            // Add fittest that will not be changed
            //newPopulation.Add(new Individual(fittestIndividual));
            // Add fittest which could be potentially changed
            //newPopulation.Add(new Individual(fittestIndividual));
            //newPopulation.Add(fittestIndividual);
            newPopulation.Add(pop.GetFittest());
            //newPopulation.Add(new Individual(population.GetFittest()));

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
        for (int i = elitismOffset; i < pop.individuals.Count; i++)
        {
            Individual individual1 = TournamentSelection(pop);
            Individual individual2 = TournamentSelection(pop);
            Individual newIndividual = UniformCrossover(individual1, individual2);

            newPopulation.Add(newIndividual);
        }

        // Mutate population
        for (int i = elitismOffset; i < pop.individuals.Count; i++)
        {
            //if (i == elitismOffset)
            //{
            //    Debug.Log("before mutation:\n");
            //    newPopulation.individuals[i].Print();
            //}
            // mutationRate % chance of mutating
            if (Random.Range(0.0f, 1.0f) <= mutationRate)
            {
                Mutate(newPopulation.individuals[i]);
            }
            //if (i == elitismOffset)
            //{
            //    Debug.Log("after mutation:\n");
            //    newPopulation.individuals[i].Print();
            //}
        }

        // Diversity check
        for (int i = elitismOffset; i < newPopulation.individuals.Count - 1; i++)
        {
            populationDiversity = newPopulation.individuals.Count;
            // Compare current individual with next in population
            if (newPopulation.individuals[i].Equals(newPopulation.individuals[i + 1]))
            {
                Debug.Log("Diversity mutation");
                // Mutate one of them if they are the same
                Mutate(newPopulation.individuals[i]);

                // Check again
                if (newPopulation.individuals[i].Equals(newPopulation.individuals[i + 1]))
                {
                    populationDiversity--;
                }
            }
        }

        //newPopulation.individuals[0] = fittestIndividual;

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
            //Vector2 position = new Vector2(Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier,
            //                               Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier);

            //Vector2 position = new Vector2(Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier,
            //                               Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier);

            //float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
            //float yMax = Mathf.CeilToInt(genomeLength / xMax);
            //Debug.Log("xMax is: " + (int)xMax + "(" + xMax + ")");
            //Debug.Log("yMax is: " + (int)yMax + "(" + yMax + ")");

            //Vector2 position = new Vector2(Random.Range(0, (int)xMax) * positionModifier,
            //                   Random.Range(0, (int)yMax) * positionModifier);

            //Vector2 position = new Vector2(i * positionModifier,
            //                               i * positionModifier);

            //float result = Mathf.Sqrt(genomeLength);
            //bool isSquare = result % 1 == 0;

            //if (!isSquare)
            //{
            //    if (genomeLength % 2 == 0)
            //    {
            //        position = new Vector2(Random.Range(0, (int)Mathf.Ceil(Mathf.Sqrt(genomeLength))) * positionModifier,
            //                               Random.Range(0, (int)Mathf.Floor(Mathf.Sqrt(genomeLength))) * positionModifier);
            //    }
            //    else
            //    {
            //        position = new Vector2(Random.Range(0, (int)Mathf.Ceil((float)genomeLength / 2)) * positionModifier,
            //                               Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier);

            //    }
            //}

            //if (position.x < 0)
            //{
            //    position.x = -Mathf.Sqrt(Mathf.Abs(position.x));
            //}
            //else
            //{
            //    position.x = Mathf.Sqrt(position.x);
            //}

            //if (position.y < 0)
            //{
            //    position.y = -Mathf.Sqrt(Mathf.Abs(position.y));
            //}
            //else
            //{
            //    position.y = Mathf.Sqrt(position.y);
            //}

            float rotation = Random.Range(0, 4);
            rotation *= 90f;
            int roomType = Random.Range(0, 7);
            LevelPiece piece = new LevelPiece(Vector2.zero, rotation, (LevelPiece.Type)roomType);
            individual.designElements.Add(piece);

        }
        population.Add(individual);
    }
    // Displays the population in Unity
    void DisplayPopulation()
    {
        if (!displaying && currentIndividual < population.individuals.Count)
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
            DisplayIndividual(population.individuals[currentIndividual]);

            displaying = true;

        }
        if (displaying)
        {
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime)
            {
                //if (currentIndividual == 0)
                //{
                // Display graph
                //Graph.Print();
                connectedComponents = Graph.CalculateConnectivity();
                GraphEditor.InitRects(genomeLength);

                //}
                // Calculate fitness for current individual
                //population.individuals[currentIndividual].fitness = overlapPenalty + (genomeLength - connectedCount);
                int maxConnections = CalculateMaxConnections(population.individuals[currentIndividual]);

                //population.individuals[currentIndividual].fitness = overlapPenalty * overlapPenaltyMultiplier + (maxConnections - connectedCount) * connectionPenaltyMultiplier;
                population.individuals[currentIndividual].fitness = connectedComponents;


                //int cross = 0, t_junction = 0, hall = 0, corner = 0, room = 0;
                //foreach (LevelPiece lp in population.individuals[currentIndividual].designElements)
                //{
                //    switch (lp.type)
                //    {
                //        case LevelPiece.Type.Cross:
                //            cross++;
                //            break;
                //        case LevelPiece.Type.T_Junction:
                //            t_junction++;
                //            break;
                //        case LevelPiece.Type.Hall:
                //            hall++;
                //            break;
                //        case LevelPiece.Type.Corner:
                //            corner++;
                //            break;
                //        case LevelPiece.Type.Room:
                //            room++;
                //            break;
                //    }
                //    if (cross >= genomeLength || t_junction >= genomeLength || hall >= genomeLength || corner >= genomeLength || room >= genomeLength)
                //    {
                //        population.individuals[currentIndividual].fitness += genomeLength;
                //    }
                //}
                populationArea = 0;
                //List<GameObject> levelPieces = CollectLevelPieces();

                //foreach (GameObject levelPiece in levelPieces)
                //{
                //    populationArea += Area(levelPiece);
                //}
                fitness = population.individuals[currentIndividual].fitness;
                // Closest to 0 is fittest
                if (fitness <= fittest)
                {
                    fittest = fitness;
                    overlap = overlapPenalty;
                    connection = (maxConnections - connectedCount);
                    fittestIndividual = new Individual(population.individuals[currentIndividual]);
                    //                    LevelGeneratorEditor.load = true;
                    //                    ScreenCapture.CaptureScreenshot("Assets/Resources/FittestLevel.png");
                    //#if UNITY_EDITOR
                    //                    AssetDatabase.Refresh();
                    //#endif
                }
                //Debug.Log("I am individual number: " + currentIndividual + ", my OVERLAP penalty is: " + overlapPenalty +
                //", my DISCONNECT penalty is: " + (populationSize - connectedCount));

                currentIndividual++;
                if (currentIndividual == populationSize)
                {
                    initialised = true;
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
            int randomId = Random.Range(0, pop.individuals.Count);
            tournamentPopulation.Add(pop.individuals[randomId]);
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
            //Debug.Log("Mutation type: " + mutationType);
            //if (mutationType == 0)
            //{
            //    MutatePosition(individual.designElements[i]);
            //}
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
        // Select a random position
        //Vector2 position = new Vector2(Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier,
        //                               Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier);
        //Vector2 position = new Vector2(Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier,
        //                               Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier);

        float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
        float yMax = Mathf.CeilToInt(genomeLength / xMax);

        Vector2 position = new Vector2(Random.Range(0, (int)xMax) * positionModifier,
                           Random.Range(0, (int)yMax) * positionModifier);

        //float result = Mathf.Sqrt(genomeLength);
        //bool isSquare = result % 1 == 0;

        //if (!isSquare)
        //{
        //    if (genomeLength % 2 == 0)
        //    {
        //        position = new Vector2(Random.Range(0, (int)Mathf.Ceil(Mathf.Sqrt(genomeLength))) * positionModifier,
        //                               Random.Range(0, (int)Mathf.Floor(Mathf.Sqrt(genomeLength))) * positionModifier);
        //    }
        //    else
        //    {
        //        position = new Vector2(Random.Range(0, (int)Mathf.Ceil((float)genomeLength / 2)) * positionModifier,
        //                               Random.Range(0, (int)Mathf.Sqrt(genomeLength)) * positionModifier);

        //    }
        //}

        // Keep re-positioning until value is different
        while (position == designElement.position)
        {
            //position = new Vector2(Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier,
            //                                   Random.Range((int)-Mathf.Sqrt(genomeLength), (int)Mathf.Sqrt(genomeLength)) * positionModifier);

            position = new Vector2(Random.Range(0, (int)xMax) * positionModifier,
                               Random.Range(0, (int)yMax) * positionModifier);
            //Debug.Log("gotta change position");
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
            //Debug.Log("gotta change rotation");
        }
        rotation *= 90f;
        //Debug.Log(rotation);
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
            //Debug.Log("gotta change room type");
        }
        levelPiece.type = (LevelPiece.Type)roomType;
    }

    void DisplayIndividual(Individual individual)
    {
        int count = 0;
        float xMax = Mathf.RoundToInt(Mathf.Sqrt(genomeLength));
        float yMax = Mathf.CeilToInt(genomeLength / xMax);
        // Initialise graph
        Graph.nodes = new List<GraphNode>();
        // Initialise trap positions list
        trapPositions = new List<Vector3>();
        for (int x = 0; x < (int)xMax; x++)
        {
            for (int y = 0; y < (int)yMax; y++)
            {
                //Debug.Log(new Vector2(x, y));
                if (count < genomeLength)
                {

                    LevelPiece piece = (LevelPiece)individual.designElements[count];
                    piece.position.x = (x * positionModifier) - xMax;
                    piece.position.y = (y * positionModifier) - yMax;

                    switch (piece.type)
                    {
                        case LevelPiece.Type.Cross:
                            GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCross.transform.parent = gameObject.transform;
                            tempCross.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempCross.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempCross.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.T_Junction:
                            GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempT_Junction.transform.parent = gameObject.transform;
                            tempT_Junction.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempT_Junction.name, count));

                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //trapPositions.Add(new Vector3(tempT_Junction.transform.forward.x, tempT_Junction.transform.forward.y, tempT_Junction.transform.forward.z) + new Vector3(-1,0,0));
                            //trapPositions.Add(tempT_Junction.transform.forward + new Vector3(2.5f, 0, 0));
                            //trapPositions.Add(tempT_Junction.transform.forward - new Vector3(2.5f, 0, 0));
                            //tempT_Junction.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.Hall:
                            GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempHall.transform.parent = gameObject.transform;
                            tempHall.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempHall.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempHall.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.Corner:
                            GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempCorner.transform.parent = gameObject.transform;
                            tempCorner.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempCorner.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempCorner.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.Room1:
                            GameObject tempRoom1 = Instantiate(room1, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom1.transform.parent = gameObject.transform;
                            tempRoom1.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempRoom1.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempRoom.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.Room2:
                            GameObject tempRoom2 = Instantiate(room2, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom2.transform.parent = gameObject.transform;
                            tempRoom2.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempRoom2.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempRoom.tag = "LevelPiece";
                            break;
                        case LevelPiece.Type.Room3:
                            GameObject tempRoom3 = Instantiate(room3, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
                            tempRoom3.transform.parent = gameObject.transform;
                            tempRoom3.name += count;
                            // Add a new node to graph
                            Graph.nodes.Add(new GraphNode(tempRoom3.name, count));
                            //trapPositions.Add(new Vector3(piece.position.x, -2.5f, piece.position.y));
                            //tempRoom.tag = "LevelPiece";
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

        //foreach (LevelPiece piece in individual.designElements)
        //{

        //    count++;
        //    switch (piece.type)
        //    {
        //        case LevelPiece.Type.Cross:
        //            GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempCross.transform.parent = gameObject.transform;
        //            tempCross.name += count;
        //            //tempCross.tag = "LevelPiece";
        //            break;
        //        case LevelPiece.Type.T_Junction:
        //            GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempT_Junction.transform.parent = gameObject.transform;
        //            tempT_Junction.name += count;
        //            //tempT_Junction.tag = "LevelPiece";
        //            break;
        //        //case LevelPiece.Type.Hall:
        //        //    GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //        //    tempHall.transform.parent = gameObject.transform;
        //        //    tempHall.name += count;
        //        //    tempHall.tag = "LevelPiece";
        //        //    break;
        //        case LevelPiece.Type.Corner:
        //            GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempCorner.transform.parent = gameObject.transform;
        //            tempCorner.name += count; //a
        //            //tempCorner.tag = "LevelPiece";
        //            break;
        //        case LevelPiece.Type.Room1:
        //            GameObject tempRoom1 = Instantiate(room1, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempRoom1.transform.parent = gameObject.transform;
        //            tempRoom1.name += count;
        //            //tempRoom.tag = "LevelPiece";
        //            break;
        //        case LevelPiece.Type.Room2:
        //            GameObject tempRoom2 = Instantiate(room2, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempRoom2.transform.parent = gameObject.transform;
        //            tempRoom2.name += count;
        //            //tempRoom.tag = "LevelPiece";
        //            break;
        //        case LevelPiece.Type.Room3:
        //            GameObject tempRoom3 = Instantiate(room3, new Vector3(piece.position.x, 0, piece.position.y), Quaternion.AngleAxis(piece.rotation, Vector3.up)) as GameObject;
        //            tempRoom3.transform.parent = gameObject.transform;
        //            tempRoom3.name += count;
        //            //tempRoom.tag = "LevelPiece";
        //            break;
        //    }
        //}
        // TODO: Fix overlap and disconnected penalties detection to work, maybe make coroutine or async?
        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(), new Quaternion());
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

}