// Simple Genetic Algorithm (SGA)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SimpleGA : GeneticAlgorithm
{
    public Population population = new Population();
    // Used to keep track of all the feasible Individuals
    public Population feasibleIndividuals = new Population();
    public int currentIndividual;
    public bool initialisedPopulation;
    public Individual fittestIndividual = new Individual();
    private int fittestGeneration;
    public float fittest = 0;
    public float fitness = 0;
    
    public SimpleGA(int populationSize, int genomeLength, float mutationRate, bool elitism, CrossoverType crossoverType, int tournamentSize, float evaluationTime, bool testing,
         int testRuns, int maxGeneration, LevelGenerator levelGenerator)
    {
        this.populationSize = populationSize;
        this.genomeLength = genomeLength;
        this.levelGenerator = levelGenerator;
        this.tournamentSize = tournamentSize;
        this.testing = testing;
        this.testRuns = testRuns;
        this.maxGeneration = maxGeneration;
        this.elitism = elitism;
        this.crossoverType = crossoverType;
        this.mutationRate = mutationRate;
        this.evaluationTime = evaluationTime;
        Initialise();
    }

    public void Initialise()
    {
        initialisedPopulation = false;
        population = new Population();
        GenerateRandomPopulation(populationSize, population);
        generation = 1;
        fittest = 0;
    }

    public void Run()
    {
        if (generation <= maxGeneration ^ (terminate || currentTestRun >= testRuns))
        {
            totalTime += Time.deltaTime;
            var minutes = Mathf.FloorToInt(totalTime / 60F);
            var seconds = Mathf.FloorToInt(totalTime - minutes * 60);
            time = $"{minutes:0}:{seconds:00}";

            // Will spawn infeasible levels and evaluate them
            DisplayPopulation(population);

            if (initialisedPopulation)
            {

                generation++;
                population = EvolvePopulation(population);
                currentIndividual = 0;
                initialisedPopulation = false;

            }
        }
        else if (!finished)
        {

            //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");
            if (testing && currentTestRun < testRuns)
            {
                currentTestRun++;
                Debug.Log("Current run produced " + feasibleIndividualCount + " feasible individuals, with a best fitness of " + fittestIndividual.fitness + ", generated at generation #" + fittestGeneration + " cc:" + connectedComponents);
                output += feasibleIndividualCount + ", " + fittestIndividual.fitness + ", " + fittestGeneration + "\n";



                // Append title to csv
                if (csv.Length == 0)
                {
                    AddDataToResults(string.Format("{0},{1},{2},{3},{4}", "Number of Feasible individuals", "Fittest Individual Fitness", "Generation of Fittest Individual", "Generation of First Feasible Individual", "Connected Components"));
                }
                // Append new line to csv
                AddDataToResults(string.Format("{0},{1},{2},{3},{4}", feasibleIndividualCount, fittestIndividual.fitness, fittestGeneration, firstFeasibleGeneration, connectedComponents));
                levelGenerator.ClearScene();

                if (currentTestRun <= testRuns - 1)
                {
                    population.individuals.Clear();
                    fittestIndividual = new Individual();
                    FitnessVisualizerEditor.values.Clear();
                    Initialise();
                }
                totalTime = 0;
                currentIndividual = 0;
                feasibleIndividualCount = 0;
                initialisedPopulation = false;
                generation = 1;
                fittest = 0;
            }
            else
            {
                Debug.Log("Clearing and spawning fittest, it has a fitness of: " + fittestIndividual.fitness);
                if (testing)
                {
                    OutputTestResults();
                    Debug.Log(output);
                }

                levelGenerator.ClearScene();
                levelGenerator.DisplayIndividual(fittestIndividual);
                // Spawn dead end walls with a delay, so there is enough time for collision detection
                levelGenerator.InvokeSpawnWallsOnDeadEnds(evaluationTime);
                finished = true;
            }
        }
    }

    private void DisplayPopulation(Population pop)
    {
        if (!displaying && currentIndividual < pop.Size())
        {
            // Reset variables

            LevelGenerator.disconnectPenalty = 0;
            LevelGenerator.connectedCount = 0;

            LevelGenerator.connected.Clear();
            // Clear scene before spawning level
            levelGenerator.ClearScene();
            // Reset cooldown
            cooldown = 0;
            // Display the individual
            levelGenerator.DisplayIndividual(pop.individuals[currentIndividual]);

            displaying = true;

        }
        if (displaying && currentIndividual < pop.Size())
        {
            cooldown += Time.deltaTime;
            if (cooldown >= evaluationTime + Time.deltaTime)
            {
                connectedComponents = LevelGenerator.graph.CalculateConnectivity();

                GraphEditor.InitRects(genomeLength);

                pop.individuals[currentIndividual].fitness = CalculateCombinedFitness(); //(genomeLength - connectedComponents) + shortestPathCost / 10 + kConnectivity;
                FitnessVisualizerEditor.values.Add(pop.individuals[currentIndividual].fitness);


                fitness = pop.individuals[currentIndividual].fitness;


                if (fitness > fittest)
                {
                    fittest = fitness;
                    fittestIndividual = Utility.DeepClone(pop.individuals[currentIndividual]);
                    fittestGeneration = generation;
                }

                // Feasible
                if (connectedComponents == 1)
                {

                    // Keep track of generation of first feasible individual
                    if(feasibleIndividualCount == 0)
                        firstFeasibleGeneration = generation;

                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.individuals.Contains(pop.individuals[currentIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop.individuals[currentIndividual]));
                    }

                }

                currentIndividual++;
                if (currentIndividual == population.Size())
                {
                    initialisedPopulation = true;
                }
                displaying = false;
            }
        }
    }
}
