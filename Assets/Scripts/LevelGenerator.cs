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
    Fi2PopGA,
    NoveltySearchGA
}

public enum CrossoverType
{
    SinglePoint,
    Uniform,
    Hybrid
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
    public Technique technique;
    public CrossoverType crossoverType;
    //public GameObject seekerSlow;
    public GameObject target;
    [ReadOnly] public int currentInfeasibleIndividual = 0;
    [ReadOnly] public int currentFeasibleIndividual = 0;
    [ReadOnly] public int generation = 1;
    [ReadOnly] public float fittestInfeasible = 0;
    [ReadOnly] public float fittestFeasible = 0;
    [ReadOnly] public float fitnessInfeasible = 0;
    [ReadOnly] public float fitnessFeasible = 0;

    [ReadOnly] public int connection = 0;

    [ReadOnly] public int connectedComponents = 0;

    public bool displayCeiling = true;

    public int genomeLength = 10;
    public int populationSize = 50;
    public float positionModifier = 15f;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.05f;
    public bool elitism = true;
    public int tournamentSize;
    public float evaluationTime = 0.1f;
    public Population infeasiblePopulation = new Population();
    public Population feasiblePopulation = new Population();
    public Population noveltyArchive = new Population();
   
    public Individual infeasibleFittest = new Individual();
    public Individual feasibleFittest = new Individual();
    
    public string time;
    public int runtimeInSeconds = 300;
    public int maxGeneration = 20;
    public bool testing;
    [ReadOnly] public bool terminate = false;

    public static int shortestPathCost = 0;
    public float cost = 0;
    public static Graph graph = new Graph();
    public int testRuns = 10;
    // Private properties
    private float cooldown = 0;
    private bool displaying = false;
    private static float uniformRate = 0.5f;
    private List<Vector3> trapPositions = new List<Vector3>();
    private bool finished = false;
    private float totalTime;
    private int feasibleIndividualCount;
    private int currentTestRun;
    private string output = "";
    private bool initialisedInfeasiblePop;
    private bool initialisedFeasiblePop;
    private int feasibleIndividualGeneration;
    private int infeasibleIndividualGeneration;
    // Use this for initialization
    void Start()
    {
        initialisedInfeasiblePop = false;
        initialisedFeasiblePop = false;
        infeasiblePopulation = new Population();
        feasiblePopulation = new Population();
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
            case Technique.NoveltySearchGA:
                NoveltySearchGA();
                break;
            default:
                break;
        }
    }

    public void FI2PopGA()
    {
        totalTime += Time.deltaTime;
        if (generation <= maxGeneration ^ (terminate || currentTestRun >= testRuns))
        {
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayInfeasiblePopulation(infeasiblePopulation);
            // Will spawn feasible levels and evaluate them
            if (feasiblePopulation.Size() >= 1 && initialisedInfeasiblePop)
            {
                DisplayFeasiblePopulation(feasiblePopulation);
            }
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

            if (testing && currentTestRun < testRuns)
            {
                Debug.Log("Current run produced " + feasibleIndividualCount + " feasible individuals, with a best fitness of " + feasibleFittest.fitness + ", generated at generation #" + feasibleIndividualGeneration);
                output += feasibleIndividualCount + ", " + fittestFeasible + ", " + feasibleIndividualGeneration + "\n";
                currentTestRun++;
                ClearScene();
                FitnessVisualizerEditor.values.Clear();
                if (currentTestRun <= testRuns - 1)
                {
                    infeasiblePopulation.individuals.Clear();
                    feasiblePopulation.individuals.Clear();
                    infeasibleFittest = new Individual();
                    feasibleFittest = new Individual();
                    Start();
                }
                totalTime = 0;
                currentInfeasibleIndividual = 0;
                currentFeasibleIndividual = 0;
                feasibleIndividualCount = 0;

                initialisedInfeasiblePop = false;
                initialisedFeasiblePop = false;
                generation = 1;
                fittestFeasible = 0;
                fittestInfeasible = 0;
            }
            else
            {
                //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");
                ClearScene();

                if(feasibleIndividualCount > 0)
                {
                    Debug.Log("Clearing and spawning fittest, it has a fitness of: " + feasibleFittest.fitness);
                    DisplayIndividual(feasibleFittest);
                }
                else
                {
                    Individual fittest = infeasiblePopulation.GetFittest();
                    Debug.Log("No feasible individual was found, clearing and spawning fittest from infeasible population, it has a fitness of: " + fittest.fitness);
                    DisplayIndividual(fittest);
                }
                if (testing)
                    Debug.Log(output);
                // Spawn dead end walls with a delay, so there is enough time for collision detection
                Invoke("SpawnWallsOnDeadEnds", evaluationTime);
                finished = true;
            }
        }
    }

    public void SimpleGA()
    {
        
        if (generation <= maxGeneration ^ (terminate || currentTestRun >= testRuns))//if (fittest != 1) //if (currentIndividual != 1)
        {
            totalTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(totalTime / 60F);
            int seconds = Mathf.FloorToInt(totalTime - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayInfeasiblePopulation(infeasiblePopulation);

            if (initialisedInfeasiblePop)
            {

                generation++;
                infeasiblePopulation = EvolvePopulation(infeasiblePopulation);
                currentInfeasibleIndividual = 0;
                initialisedInfeasiblePop = false;

            }
        }
        else if (!finished)
        {

            //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");
            if (testing && currentTestRun < testRuns)
            {
                currentTestRun++;
                Debug.Log("Current run produced " + feasibleIndividualCount + " feasible individuals, with a best fitness of " + infeasibleFittest.fitness + ", generated at generation #" + infeasibleIndividualGeneration);
                output += feasibleIndividualCount + ", " + infeasibleFittest.fitness + ", " + infeasibleIndividualGeneration + "\n";
                ClearScene();
                FitnessVisualizerEditor.values.Clear();
                if (currentTestRun <= testRuns - 1)
                {
                    infeasiblePopulation.individuals.Clear();
                    feasiblePopulation.individuals.Clear();
                    infeasibleFittest = new Individual();
                    feasibleFittest = new Individual();
                    Start();
                }
                totalTime = 0;
                currentInfeasibleIndividual = 0;
                currentFeasibleIndividual = 0;
                feasibleIndividualCount = 0;

                initialisedInfeasiblePop = false;
                initialisedFeasiblePop = false;
                generation = 1;
                fittestFeasible = 0;
                fittestInfeasible = 0;
            }
            else
            {

                Debug.Log("Clearing and spawning fittest, it has a fitness of: " + infeasiblePopulation.GetFittest().fitness);
                if (testing)
                    Debug.Log(output);
                ClearScene();

                // Show fittest individual
                //DisplayIndividual(final);
                DisplayIndividual(infeasiblePopulation.GetFittest());
                // Spawn dead end walls with a delay, so there is enough time for collision detection
                Invoke("SpawnWallsOnDeadEnds", evaluationTime);
                finished = true;
            }
        }
    }

    public void NoveltySearchGA()
    {
        if (Time.time <= runtimeInSeconds ^ terminate)
        {
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayInfeasiblePopulation(infeasiblePopulation);

            if (initialisedInfeasiblePop)
            {

                generation++;
                noveltyArchive.Add(Utility.DeepClone(infeasiblePopulation.GetFittest()));
                infeasiblePopulation = EvolvePopulation(infeasiblePopulation);

                currentInfeasibleIndividual = 0;
                initialisedInfeasiblePop = false;

            }
        }
        else if (!finished)
        {
            Debug.Log("Clearing and spawning fittest, it has a fitness of: " + infeasiblePopulation.GetFittest());
            ClearScene();

            DisplayIndividual(infeasiblePopulation.GetFittest());
            Invoke("SpawnWallsOnDeadEnds", evaluationTime);
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
            newPopulation.Add(Utility.DeepClone(pop.GetFittest()));
            // Add fittest to new population a second time, will be affected by mutation
            newPopulation.Add(Utility.DeepClone(pop.GetFittest()));

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
            switch (crossoverType)
            {
                case CrossoverType.SinglePoint:
                    newIndividual = SinglePointCrossover(individual1, individual2);
                    break;
                case CrossoverType.Uniform:
                    newIndividual = UniformCrossover(individual1, individual2);
                    break;
                case CrossoverType.Hybrid:
                    // Perform uniform crossover every 3 generations, for the rest use single point crossover
                    if (generation % 3 == 0)
                        newIndividual = UniformCrossover(individual1, individual2);
                    else
                        newIndividual = SinglePointCrossover(individual1, individual2);
                    break;
                default:
                    break;
            }

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
            int roomType = Random.Range(0, 16);
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
        if (displaying && currentInfeasibleIndividual < pop.Size())
        {
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime)
            {

                connectedComponents = graph.CalculateConnectivity();

                GraphEditor.InitRects(genomeLength);

                switch (technique)
                {
                    case Technique.SimpleGA:
                        pop.individuals[currentInfeasibleIndividual].fitness = CalculateCombinedFitness(); //(genomeLength - connectedComponents) + shortestPathCost / 10 + kConnectivity;
                        FitnessVisualizerEditor.values.Add(pop.individuals[currentInfeasibleIndividual].fitness);
                        break;
                    case Technique.Fi2PopGA:
                        pop.individuals[currentInfeasibleIndividual].fitness = CalculateConstraintFitness();
                        break;
                    case Technique.NoveltySearchGA:
                        int averageDiversity = 0;
                        int divisor = 0;
                        // Compare current individual to neighbours in the population
                        for (int neighbour = 0; neighbour < 2; neighbour++)
                        {
                            if (currentInfeasibleIndividual < pop.Size() - 1)
                            {
                                //Debug.Log(currentInfeasibleIndividual);
                                averageDiversity += pop.individuals[currentInfeasibleIndividual].GetDiversity(pop.individuals[currentInfeasibleIndividual + neighbour]);
                                divisor++;
                            }

                            if (currentInfeasibleIndividual > 0)
                            {
                                averageDiversity += pop.individuals[currentInfeasibleIndividual].GetDiversity(pop.individuals[currentInfeasibleIndividual - neighbour]);
                                divisor++;
                            }
                        }
                        // Compare current individual with novelty archive
                        for (int i = 0; i < noveltyArchive.Size(); i++)
                        {
                            averageDiversity += pop.individuals[currentInfeasibleIndividual].GetDiversity(noveltyArchive.individuals[i]);
                            divisor++;
                        }

                        averageDiversity /= divisor;
                        pop.individuals[currentInfeasibleIndividual].fitness = averageDiversity;
                        break;
                    default:
                        break;
                }

                fitnessInfeasible = pop.individuals[currentInfeasibleIndividual].fitness;
                
                if (fitnessInfeasible >= fittestInfeasible)// && !pop.individuals[currentIndividual].Equals(fittestIndividual))
                {
                    fittestInfeasible = fitnessInfeasible;
                    infeasibleFittest = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);
                    if (technique == Technique.NoveltySearchGA)
                    {
                        cost = CalculateObjectiveFitness();
                    }
                }

                // Feasible
                if (connectedComponents == 1)
                {
                    //Debug.Break();
                    // Add to feasible population only if solution is different
                    if (!feasiblePopulation.individuals.Contains(pop.individuals[currentInfeasibleIndividual]))
                    {
                      //Debug.Break();
                      feasibleIndividualCount++;
                      Individual feasibleIndividual = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);
                      feasiblePopulation.Add(feasibleIndividual);
                      infeasibleIndividualGeneration = generation;
                      pop.individuals[currentInfeasibleIndividual].delete = true;
                    }
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
        if (displaying && currentFeasibleIndividual < pop.Size())
        {   
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime)
            {
                
                // Calculates the 2-vertex-connectivity of the graph
                //int kConnectivity = graph.CalculateVariableKConnectivity();
                

                GraphEditor.InitRects(genomeLength);
                connectedComponents = graph.CalculateConnectivity();

                pop.individuals[currentFeasibleIndividual].fitness = CalculateCombinedFitness();

                // Infeasible
                if (connectedComponents != 1)
                {
                    // Since individual has now become infeasible, set it's fitness to the constraint fitness
                    pop.individuals[currentFeasibleIndividual].fitness = CalculateConstraintFitness();
                    // Move to infeasible population
                    infeasiblePopulation.Add(Utility.DeepClone(pop.individuals[currentFeasibleIndividual]));
                    // Set to delete
                    pop.individuals[currentFeasibleIndividual].delete = true;
                }
                else
                {
                    fitnessFeasible = pop.individuals[currentFeasibleIndividual].fitness;
                    FitnessVisualizerEditor.values.Add(fitnessFeasible);
                    if (fitnessFeasible >= fittestFeasible)
                    {
                        feasibleIndividualGeneration = generation;
                        fittestFeasible = fitnessFeasible;
                        feasibleFittest = Utility.DeepClone(pop.individuals[currentFeasibleIndividual]);
                    }
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

    public float CalculateCombinedFitness()
    {
        // Normalized path cost
        float maxNodes = 144;
        float minNodes = 0; //minNodes = 12;
        float maxPathCost = (genomeLength - 1) * maxNodes;
        float minPathCost = ((genomeLength / 2) - 1) * minNodes;
        float normalizedShortestPathCost = (shortestPathCost - minPathCost) / (maxPathCost - minPathCost);
        // Normalized connected components
        float maxConnectedComponents = genomeLength - 1;
        float minConnectedComponents = 0;
        float connectedComponentsScore = genomeLength - connectedComponents;
        float normalizedConnectedComponentsScore = (connectedComponentsScore - minConnectedComponents) / (maxConnectedComponents - minConnectedComponents);
        // Normalized K-Vertex-Connectivity
        int kConnectivity = graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        float minKVertexConnectivity = 0;
        float maxKVertexConnectivity = genomeLength * 2;
        float normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return (normalizedShortestPathCost * 1.5f + normalizedConnectedComponentsScore * 0.5f + normalizedKVertexConnectivity * 1.5f) / 3;
    }

    public float CalculateConstraintFitness()
    {
        // Normalized connected components
        float maxConnectedComponents = genomeLength - 1;
        float minConnectedComponents = 0;
        float connectedComponentsScore = genomeLength - connectedComponents;
        float normalizedConnectedComponentsScore = (connectedComponentsScore - minConnectedComponents) / (maxConnectedComponents - minConnectedComponents);

        return normalizedConnectedComponentsScore;
    }

    public float CalculateObjectiveFitness()
    {
        // Normalized path cost
        float maxNodes = 144;
        float minNodes = 0; //minNodes = 12;
        float maxPathCost = (genomeLength - 1) * maxNodes;
        float minPathCost = ((genomeLength / 2) - 1) * minNodes;
        float normalizedShortestPathCost = (shortestPathCost - minPathCost) / (maxPathCost - minPathCost);

        // Normalized K-Vertex-Connectivity
        int kConnectivity = graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        float minKVertexConnectivity = 0;
        float maxKVertexConnectivity = genomeLength * 2;
        float normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return (normalizedShortestPathCost * 0.5f + normalizedKVertexConnectivity * 1.5f) / 2;
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
        int roomType = Random.Range(0, 8);
        // Keep chaning piece until it's different
        while ((LevelPiece.Type)roomType == levelPiece.type)
        {
            roomType = Random.Range(0, 16);
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
        Instantiate(target, furthest, new Quaternion());
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

    void HideCeiling(bool hide)
    {
        GameObject[] ceilings = GameObject.FindGameObjectsWithTag("Ceiling");
        foreach (GameObject ceiling in ceilings)
        {
            ceiling.SetActive(!hide);
        }
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