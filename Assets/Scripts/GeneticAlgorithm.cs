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

    public Population EvolvePopulation(Population pop)
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
    public void GenerateRandomPopulation(int size, Population pop)
    {
        for (int i = 0; i < size; i++)
        {
            GenerateRandomIndividual(pop);
        }
    }
    // Generates a random individual
    public void GenerateRandomIndividual(Population pop)
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
        pop.Add(individual);
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

    public float CalculateCombinedFitness()
    {
        // Normalized path cost
        float maxNodes = 144;
        float minNodes = 0; //minNodes = 12;
        float maxPathCost = (genomeLength - 1) * maxNodes;
        float minPathCost = ((genomeLength / 2) - 1) * minNodes;
        float normalizedShortestPathCost = (LevelGenerator.shortestPathCost - minPathCost) / (maxPathCost - minPathCost);
        // Normalized connected components
        float maxConnectedComponents = genomeLength - 1;
        float minConnectedComponents = 0;
        float connectedComponentsScore = genomeLength - connectedComponents;
        float normalizedConnectedComponentsScore = (connectedComponentsScore - minConnectedComponents) / (maxConnectedComponents - minConnectedComponents);
        // Normalized K-Vertex-Connectivity
        int kConnectivity = LevelGenerator.graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        float minKVertexConnectivity = 0;
        float maxKVertexConnectivity = genomeLength * 2;
        float normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return (normalizedShortestPathCost * 1.5f + normalizedConnectedComponentsScore * 0.5f + normalizedKVertexConnectivity * 1.5f) / 3;
    }

    public float CalculateKVertexConnectivtyFitness()
    {
        // Normalized K-Vertex-Connectivity
        int kConnectivity = LevelGenerator.graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        float minKVertexConnectivity = 0;
        float maxKVertexConnectivity = genomeLength * 2;
        float normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return normalizedKVertexConnectivity;
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
        float normalizedShortestPathCost = (LevelGenerator.shortestPathCost - minPathCost) / (maxPathCost - minPathCost);

        // Normalized K-Vertex-Connectivity
        int kConnectivity = LevelGenerator.graph.CalculateKConnectivity(2);//graph.CalculateVariableKConnectivity();
        float minKVertexConnectivity = 0;
        float maxKVertexConnectivity = genomeLength * 2;
        float normalizedKVertexConnectivity = (kConnectivity - minKVertexConnectivity) / (maxKVertexConnectivity - minKVertexConnectivity);

        return (normalizedShortestPathCost * 0.5f + normalizedKVertexConnectivity * 1.5f) / 2;
    }

    public void AddDataToResults(string data)
    {
        csv.AppendLine(data);
    }

    public void OutputTestResults()
    {
        File.WriteAllText(Application.dataPath + "/Experiments/" + csvFileName + ".csv", csv.ToString());
    }
}
