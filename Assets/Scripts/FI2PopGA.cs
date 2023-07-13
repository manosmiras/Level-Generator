// Feasible Infeasible Two Population Genetic Algorithm (FI-2Pop GA)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FI2PopGA : GeneticAlgorithm
{
    public Population infeasiblePopulation = new Population();
    public Population feasiblePopulation = new Population();

    // Used to keep track of all the feasible Individuals
    public Population feasibleIndividuals = new Population();

    private bool initialisedInfeasiblePop;
    private bool initialisedFeasiblePop;
    private int feasibleIndividualGeneration;

    public Individual infeasibleFittest = new Individual();
    public Individual feasibleFittest = new Individual();

    public float fittestInfeasible = 0;
    public float fittestFeasible = 0;
    public float fitnessInfeasible = 0;
    public float fitnessFeasible = 0;
    public int currentFeasibleIndividual = 0;
    public int currentInfeasibleIndividual = 0;

    public FI2PopGA(int populationSize, int genomeLength, float mutationRate, bool elitism, CrossoverType crossoverType, int tournamentSize, float evaluationTime, bool testing,
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
        initialisedInfeasiblePop = false;
        initialisedFeasiblePop = false;
        infeasiblePopulation = new Population();
        feasiblePopulation = new Population();
        GenerateRandomPopulation(populationSize, infeasiblePopulation);

        generation = 1;
        fittestFeasible = 0;
        fittestInfeasible = 0;
    }
    public void Run()
    {
        totalTime += Time.deltaTime;
        if (generation <= maxGeneration ^ (terminate || currentTestRun >= testRuns))
        {
            var minutes = Mathf.FloorToInt(Time.time / 60F);
            var seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = $"{minutes:0}:{seconds:00}";

            // Will spawn infeasible levels and evaluate them
            if(!initialisedInfeasiblePop)
                DisplayInfeasiblePopulation(infeasiblePopulation);

            // Will spawn feasible levels and evaluate them
            if (feasiblePopulation.Size() >= 1 && initialisedInfeasiblePop)
            {
                DisplayFeasiblePopulation(feasiblePopulation);
            }
            if (initialisedInfeasiblePop && (initialisedFeasiblePop || feasiblePopulation.Size() == 0))
            {
                // Remove weakest individuals from infeasible population to maintain population size
                while (infeasiblePopulation.Size() > populationSize)
                {
                    infeasiblePopulation.individuals.RemoveAt(infeasiblePopulation.GetWeakestIndex());
                }

                // Delete individuals in feasible population which became infeasible from evolution
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

                // Append title to csv
                if (csv.Length == 0)
                {
                    AddDataToResults(string.Format("{0},{1},{2},{3}", "Number of Feasible individuals", "Fittest Individual Fitness", "Generation of Fittest Individual", "Generation of First Feasible Individual"));
                }
                // Append new line to csv
                AddDataToResults(string.Format("{0},{1},{2},{3}", feasibleIndividualCount, feasibleFittest.fitness, feasibleIndividualGeneration, firstFeasibleGeneration));

                levelGenerator.ClearScene();

                if (currentTestRun <= testRuns - 1)
                {
                    infeasiblePopulation.individuals.Clear();
                    feasiblePopulation.individuals.Clear();
                    infeasibleFittest = new Individual();
                    feasibleFittest = new Individual();
                    feasibleIndividualCount = 0;
                    FitnessVisualizerEditor.values.Clear();
                    FitnessVisualizerEditor.values2.Clear();
                    Initialise();
                }
                totalTime = 0;
                currentInfeasibleIndividual = 0;
                currentFeasibleIndividual = 0;


                initialisedInfeasiblePop = false;
                initialisedFeasiblePop = false;
                generation = 1;
                fittestFeasible = 0;
                fittestInfeasible = 0;
            }
            else
            {
                levelGenerator.ClearScene();

                if (feasibleIndividualCount > 0)
                {
                    Debug.Log("Clearing and spawning fittest, it has a fitness of: " + feasibleFittest.fitness);
                    levelGenerator.DisplayIndividual(feasibleFittest);
                }
                else
                {
                    var fittest = infeasiblePopulation.GetFittest();
                    Debug.Log("No feasible individual was found, clearing and spawning fittest from infeasible population, it has a fitness of: " + fittest.fitness);
                    levelGenerator.DisplayIndividual(fittest);
                }
                if (testing)
                {
                    OutputTestResults();
                    Debug.Log(output);
                }
                // Spawn dead end walls with a delay, so there is enough time for collision detection
                levelGenerator.InvokeSpawnWallsOnDeadEnds(evaluationTime);
                finished = true;
            }
        }
    }
    // Displays the infeasible population in Unity
    void DisplayInfeasiblePopulation(Population pop)
    {
        if (!displaying && currentInfeasibleIndividual < pop.Size())
        {
            // Reset variables
            LevelGenerator.overlapPenalty = 0;
            LevelGenerator.disconnectPenalty = 0;
            LevelGenerator.connectedCount = 0;
            LevelGenerator.overlapping.Clear();
            LevelGenerator.connected.Clear();
            // Clear scene before spawning level
            levelGenerator.ClearScene();
            // Reset cooldown
            cooldown = 0;
            // Display the individual
            levelGenerator.DisplayIndividual(pop.individuals[currentInfeasibleIndividual]);

            displaying = true;

        }
        if (displaying && currentInfeasibleIndividual < pop.Size())
        {
            cooldown += Time.deltaTime;
            if (cooldown > evaluationTime + Time.deltaTime)
            {

                connectedComponents = LevelGenerator.graph.CalculateConnectivity();

                GraphEditor.InitRects(genomeLength);


                pop.individuals[currentInfeasibleIndividual].fitness = CalculateConstraintFitness();
                FitnessVisualizerEditor.values2.Add(pop.individuals[currentInfeasibleIndividual].fitness);

                fitnessInfeasible = pop.individuals[currentInfeasibleIndividual].fitness;

                if (fitnessInfeasible >= fittestInfeasible)
                {
                    fittestInfeasible = fitnessInfeasible;
                    infeasibleFittest = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);
                }

                // Feasible
                if (connectedComponents == 1)
                {
                    // Keep track of generation of first feasible individual
                    if (feasibleIndividualCount == 0)
                        firstFeasibleGeneration = generation;

                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.individuals.Contains(pop.individuals[currentInfeasibleIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]));
                    }
                    // Create a copy with the new feasible individual
                    Individual feasibleIndividual = Utility.DeepClone(pop.individuals[currentInfeasibleIndividual]);

                    if (feasiblePopulation.Size() < populationSize)
                    {
                        // Simply add to feasible population
                        feasiblePopulation.Add(feasibleIndividual);
                    }
                    else
                    {
                        // Replace weakest feasible individual with new feasible individual
                        if (feasiblePopulation.GetWeakest().fitness > CalculateCombinedFitness())
                            feasiblePopulation.individuals[feasiblePopulation.GetWeakestIndex()] = feasibleIndividual;
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
            LevelGenerator.connectedCount = 0;
            LevelGenerator.connected.Clear();
            // Clear scene before spawning level
            levelGenerator.ClearScene();
            // Reset cooldown
            cooldown = 0;
            // Display the individual
            levelGenerator.DisplayIndividual(pop.individuals[currentFeasibleIndividual]);

            displaying = true;

        }
        if (displaying && currentFeasibleIndividual < pop.Size())
        {
            cooldown += Time.deltaTime;
            if (cooldown > evaluationTime + Time.deltaTime)
            {
                GraphEditor.InitRects(genomeLength);
                connectedComponents = LevelGenerator.graph.CalculateConnectivity();

                pop.individuals[currentFeasibleIndividual].fitness = CalculateCombinedFitness();

                // Infeasible
                if (connectedComponents != 1)
                {
                    // Since individual has now become infeasible, set it's fitness to the constraint fitness
                    pop.individuals[currentFeasibleIndividual].fitness = CalculateConstraintFitness();//(CalculateConstraintFitness() * 0.5f + CalculateKVertexConnectivtyFitness() * 1.5f) / 2;
                    // Move to infeasible population
                    infeasiblePopulation.Add(Utility.DeepClone(pop.individuals[currentFeasibleIndividual]));
                    // Set to delete
                    pop.individuals[currentFeasibleIndividual].delete = true;
                }
                else
                {
                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.individuals.Contains(pop.individuals[currentFeasibleIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop.individuals[currentFeasibleIndividual]));
                    }

                    fitnessFeasible = pop.individuals[currentFeasibleIndividual].fitness;
                    FitnessVisualizerEditor.values.Add(fitnessFeasible);
                    if (fitnessFeasible > fittestFeasible)
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


}
