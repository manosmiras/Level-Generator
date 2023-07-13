// Novelty Search (NS) and Minimal Criteria Novelty Search (MCNS)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoveltySearchGA : GeneticAlgorithm
{
    public Population population = new Population();
    public Population noveltyArchive = new Population();

    // Used to keep track of all the feasible Individuals
    public Population feasibleIndividuals = new Population();

    public int currentIndividual;
    public bool initialisedPopulation;
    public Individual fittestIndividualDiversity = new Individual();
    public Individual fittestIndividualActual = new Individual();
    private int fittestGeneration;
    public float fittest = 0;
    public float fitness = 0;
    public bool minimalCriteria;
    public bool deathPenalty;
    public NoveltySearchGA(bool minimalCriteria, bool deathPenalty, int populationSize, int genomeLength, float mutationRate, bool elitism, CrossoverType crossoverType, int tournamentSize, float evaluationTime, bool testing,
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
        this.minimalCriteria = minimalCriteria;
        this.deathPenalty = deathPenalty;
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
            int minutes = Mathf.FloorToInt(totalTime / 60F);
            int seconds = Mathf.FloorToInt(totalTime - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            DisplayPopulation(population);

            if (initialisedPopulation)
            {
                noveltyArchive.Add(Utility.DeepClone(population.GetFittestIndividual()));
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
                Debug.Log("Current run produced " + feasibleIndividualCount + " feasible individuals, with a best fitness score of " + 
                    fittestIndividualActual.fitness + " and a diversity score of " + fittestIndividualDiversity.fitness + ", generated at generation #" + fittestGeneration);
                output += feasibleIndividualCount + ", " + fittestIndividualDiversity.fitness + ", " + fittestGeneration + "\n";

                // Append title to csv
                if (csv.Length == 0)
                {
                    AddDataToResults(string.Format("{0},{1},{2},{3},{4}", "Number of Feasible individuals", "Fittest Individual Fitness", "Fittest Individual Novelty", "Generation of Fittest Individual", "Generation of First Feasible Individual"));
                }
                // Append new line to csv
                AddDataToResults(string.Format("{0},{1},{2},{3},{4}", feasibleIndividualCount, fittestIndividualActual.fitness, fittestIndividualDiversity.fitness, fittestGeneration, firstFeasibleGeneration));

                levelGenerator.ClearScene();

                if (currentTestRun <= testRuns - 1)
                {
                    population.individuals.Clear();

                    fittestIndividualDiversity = new Individual();
                    fittestIndividualActual = new Individual();
                    FitnessVisualizerEditor.values.Clear();
                    FitnessVisualizerEditor.values2.Clear();
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

                Debug.Log("Clearing and spawning fittest, it has a fitness of: " + fittestIndividualActual.fitness);
                if (testing)
                {
                    OutputTestResults();
                    Debug.Log(output);
                }
                levelGenerator.ClearScene();

                levelGenerator.DisplayIndividual(fittestIndividualActual);
                // Spawn dead end walls with a delay, so there is enough time for collision detection
                levelGenerator.InvokeSpawnWallsOnDeadEnds(evaluationTime);
                finished = true;
            }
        }
    }

    void DisplayPopulation(Population pop)
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

                // Calculate fitness
                float averageDiversity = 0;
                int divisor = 0;
                // Compare current individual to neighbours in the population
                for (int neighbour = 0; neighbour < 2; neighbour++)
                {
                    if (currentIndividual < pop.Size() - 1)
                    {
                        //Debug.Log(currentInfeasibleIndividual);
                        averageDiversity += pop.individuals[currentIndividual].GetDiversity(pop.individuals[currentIndividual + neighbour]);
                        //Debug.Log(pop.individuals[currentIndividual].GetDiversity(pop.individuals[currentIndividual + neighbour]));
                        divisor++;
                    }

                    if (currentIndividual > 0)
                    {
                        averageDiversity += pop.individuals[currentIndividual].GetDiversity(pop.individuals[currentIndividual - neighbour]);
                        divisor++;
                    }
                }
                // Compare current individual with novelty archive
                for (int i = 0; i < noveltyArchive.Size(); i++)
                {
                    averageDiversity += pop.individuals[currentIndividual].GetDiversity(noveltyArchive.individuals[i]);
                    divisor++;
                }

                averageDiversity /= divisor;
                pop.individuals[currentIndividual].fitness = averageDiversity;
                // MCNS
                if (minimalCriteria)
                {
                    // Penalty if not feasible
                    if (connectedComponents != 1)
                    {
                        if (deathPenalty)
                        {
                            // Death penalty
                            pop.individuals[currentIndividual].fitness = 0;
                        }
                        else
                        {
                            // Penalty based on distance from feasibility
                            pop.individuals[currentIndividual].fitness *= CalculateConstraintFitness();
                        }
                    }
                }
                //Debug.Log(averageDiversity);

                //pop.individuals[currentIndividual].fitness = CalculateCombinedFitness(); //(genomeLength - connectedComponents) + shortestPathCost / 10 + kConnectivity;
                FitnessVisualizerEditor.values.Add(pop.individuals[currentIndividual].fitness);
                float actualFitness = CalculateCombinedFitness();
                FitnessVisualizerEditor.values2.Add(CalculateCombinedFitness());

                fitness = actualFitness;

                if (fitness > fittest)
                {
                    fittest = fitness;
                    // Keep a copy of the fittest individual, fitness based on diversity score
                    fittestIndividualDiversity = Utility.DeepClone(pop.individuals[currentIndividual]);
                    // Keep a copy of the fittest individual, fitness based on actual combined fitness
                    fittestIndividualActual = Utility.DeepClone(pop.individuals[currentIndividual]);
                    fittestIndividualActual.fitness = actualFitness;

                }

                // Feasible
                if (connectedComponents == 1)
                {
                    // Keep track of generation of first feasible individual
                    if (feasibleIndividualCount == 0)
                        firstFeasibleGeneration = generation;

                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.individuals.Contains(pop.individuals[currentIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop.individuals[currentIndividual]));
                    }

                    fittestGeneration = generation;
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
