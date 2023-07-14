// Genetic Algorithm base class

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public abstract class GeneticAlgorithm
{
    public enum CrossoverType
    {
        SinglePoint,
        Uniform,
        Hybrid
    }
    public bool generated = false;
    public bool elitism = true;
    public CrossoverType crossoverType = CrossoverType.Hybrid;
    public int generation;
    public float mutationRate = 0.05f;
    public int genomeLength;
    public int tournamentSize;
    public GameObject target;
    public bool displayCeiling = true;
    public float totalTime;
    public int maxGeneration;
    public bool terminate;
    public int currentTestRun;
    public int testRuns;
    public float evaluationTime = 0.1f;
    public string time;
    public bool testing;
    public string output = "";
    public bool finished = false;
    public int feasibleIndividualCount;
    public bool displaying;
    public float cooldown;
    public int connectedComponents;
    public int populationSize;
    public LevelGenerator levelGenerator;

    private float uniformRate = 0.5f;

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
    public GameObject seekerFast;
    public GameObject seekerSafe;
    public GameObject seekerDangerous;

    protected int firstFeasibleGeneration;
    protected StringBuilder csv = new StringBuilder();
    public string csvFileName;

    protected Population EvolvePopulation(Population pop)
    {
        var newPopulation = new Population();

        // Keep our best individuals
        if (elitism)
        {
            if (pop.Size() != 0)
            {
                // Add fittest to new population, will not be affected by crossover or mutation
                newPopulation.Add(Utility.DeepClone(pop.GetFittestIndividual()));
                // Add fittest to new population a second time, will be affected by mutation
                newPopulation.Add(Utility.DeepClone(pop.GetFittestIndividual()));
            }
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
        var individual2 = TournamentSelection(pop);
        for (var i = elitismOffset + 1; i < pop.Size(); i++)
        {
            var individual1 = TournamentSelection(pop);
            var newIndividual = new Individual();
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
        for (var i = elitismOffset; i < pop.Size(); i++)
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
    protected void GenerateRandomPopulation(int size, Population pop)
    {
        for (var i = 0; i < size; i++)
        {
            GenerateRandomIndividual(pop);
        }
    }
    // Generates a random individual
    private void GenerateRandomIndividual(Population pop)
    {
        var individual = new Individual();

        // Clear
        individual.levelPieces.Clear();
        for (var i = 0; i < genomeLength; i++)
        {
            float rotation = Random.Range(0, 4);
            rotation *= 90f;
            var roomType = Random.Range(0, 16);
            // No traps allowed on start and end of level
            if(i == 0 || i == genomeLength - 1)
                roomType = Random.Range(0, 8);
            var piece = new LevelPiece(Vector2.zero, rotation, (LevelPiece.Type)roomType);
            individual.levelPieces.Add(piece);

        }
        pop.Add(individual);
    }

    Individual UniformCrossover(Individual individual1, Individual individual2)
    {
        var offspring = new Individual();
        // Loop through all design elements
        for (int i = 0; i < individual1.levelPieces.Count; i++)
        {
            if (individual1.fitness > individual2.fitness)
            {
                var rand = Random.Range(0.0f, 1.0f);
                // 60% chance to copy design element from individual 1
                if (rand <= uniformRate)
                {
                    offspring.levelPieces.Add(individual1.levelPieces[i]);
                }
                // 40% chance to copy design element from individual 2
                else
                {
                    offspring.levelPieces.Add(individual2.levelPieces[i]);
                }
            }
            else
            {
                var rand = Random.Range(0.0f, 1.0f);
                // 60% chance to copy design element from individual 2
                if (rand <= uniformRate)
                {
                    offspring.levelPieces.Add(individual2.levelPieces[i]);
                }
                // 40% chance to copy design element from individual 1
                else
                {
                    offspring.levelPieces.Add(individual1.levelPieces[i]);
                }
            }
        }
        return offspring;
    }

    Individual SinglePointCrossover(Individual individual1, Individual individual2)
    {
        var offspring = new Individual();
        var point = genomeLength / 2;
        var rand = Random.Range(0.0f, 1.0f);
        // Loop through all design elements
        for (var i = 0; i < individual1.levelPieces.Count; i++)
        {
            if (rand <= 0.5)
            {
                // Copies half of individual 1 and half of individual 2
                if (i <= point)
                {
                    offspring.levelPieces.Add(individual1.levelPieces[i]);
                }
                else
                {
                    offspring.levelPieces.Add(individual2.levelPieces[i]);
                }
            }
            else
            {
                // Copies half of individual 1 and half of individual 2
                if (i <= point)
                {
                    offspring.levelPieces.Add(individual2.levelPieces[i]);
                }
                else
                {
                    offspring.levelPieces.Add(individual1.levelPieces[i]);
                }
            }
        }
        return offspring;
    }

    private Individual TournamentSelection(Population pop)
    {
        // Create a tournament population
        var tournamentPopulation = new Population();

        // For each place in the tournament get a random individual
        for (var i = 0; i < tournamentSize; i++)
        {
            var randomId = Random.Range(0, pop.Size());
            tournamentPopulation.Add(Utility.DeepClone(pop.individuals[randomId]));
        }
        // Get the fittest
        var fittest = tournamentPopulation.GetFittestIndividual();

        return fittest;
    }

    // Mutate an individual
    private void Mutate(Individual individual)
    {
        // Loop through genes
        for (var i = 0; i < individual.levelPieces.Count; i++)
        {

            var mutationType = Random.Range(0, 2);
            if (mutationType == 0)
            {
                MutateRotation(individual.levelPieces[i]);
            }
            else if (mutationType == 1)
            {
                MutateLevelPiece((LevelPiece)individual.levelPieces[i], i);
            }

        }
    }

    private void MutateRotation(LevelPiece levelPiece)
    {
        // Select a random rotation between 0, 90, 180, 270 degrees
        float rotation = Random.Range(0, 4);
        // Keep rotating until value is different
        while (rotation * 90f == levelPiece.rotation)
        {
            rotation = Random.Range(0, 4);
        }
        rotation *= 90f;
        levelPiece.rotation = rotation;
    }

    void MutateLevelPiece(LevelPiece levelPiece, int i)
    {
        // Select random room type
        var roomType = Random.Range(0, 16);
        // Keep changing piece until it's different
        while ((LevelPiece.Type)roomType == levelPiece.type)
        {
            roomType = Random.Range(0, 16);
        }
        // No traps allowed on start and end of level
        if (i == 0 || i == genomeLength - 1)
            roomType = Random.Range(0, 8);

        levelPiece.type = (LevelPiece.Type)roomType;
    }

    protected float CalculateCombinedFitness()
    {
        // Normalized path cost
        var normalizedShortestPathCost = CalculatePathFitness();
        var normalizedConnectedComponentsScore = CalculateConstraintFitness();
        var normalizedKVertexConnectivity = CalculateKVertexConnectivityFitness();
        return (normalizedShortestPathCost * 1.0f + normalizedConnectedComponentsScore * 1.0f + normalizedKVertexConnectivity * 1.0f) / 3;
    }

    private float CalculateKVertexConnectivityFitness()
    {
        // Normalized K-Vertex-Connectivity
        var kConnectivity = LevelGenerator.graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        var minKVertexConnectivity = 0;
        var maxKVertexConnectivity = genomeLength * 2;
        var normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return normalizedKVertexConnectivity;
    }

    public float CalculateVariableVertexConnectivityFitness()
    {
        // Normalized K-Vertex-Connectivity
        var kConnectivity = LevelGenerator.graph.CalculateVariableKConnectivity();
        var minKVertexConnectivity = 0;
        var maxKVertexConnectivity = genomeLength * 4;
        var normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return normalizedKVertexConnectivity;
    }

    protected float CalculateConstraintFitness()
    {
        // Normalized connected components
        var maxConnectedComponents = genomeLength - 1;
        var minConnectedComponents = 0;
        var connectedComponentsScore = genomeLength - connectedComponents;
        var normalizedConnectedComponentsScore = (connectedComponentsScore - minConnectedComponents) / (maxConnectedComponents - minConnectedComponents);

        return normalizedConnectedComponentsScore;
    }

    private float CalculatePathFitness(int minNodes = 0, int maxNodes = 144)
    {
        // Normalized path cost
        var maxPathCost = (genomeLength - 1) * maxNodes;
        var minPathCost = (genomeLength / 2 - 1) * minNodes;
        var normalizedShortestPathCost = (LevelGenerator.shortestPathCost - minPathCost) / (maxPathCost - minPathCost);

        return normalizedShortestPathCost;
    }

    public float CalculateObjectiveFitness(int minNodes = 0, int maxNodes = 144)
    {
        // Normalized path cost
        var maxPathCost = (genomeLength - 1) * maxNodes;
        var minPathCost = (genomeLength / 2 - 1) * minNodes;
        var normalizedShortestPathCost = (LevelGenerator.shortestPathCost - minPathCost) / (maxPathCost - minPathCost);

        // Normalized K-Vertex-Connectivity
        var kConnectivity = LevelGenerator.graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        var minKVertexConnectivity = 0;
        var maxKVertexConnectivity = genomeLength * 2;
        var normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return (normalizedShortestPathCost * 1.0f + normalizedKVertexConnectivity * 1.0f) / 2;
    }

    protected void AddDataToResults(string data)
    {
        csv.AppendLine(data);
    }

    protected void OutputTestResults()
    {
        File.WriteAllText(Application.dataPath + "/Experiments/" + csvFileName + ".csv", csv.ToString());
    }
}
