using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FI2PopNsGA : GeneticAlgorithm
{
    public Population infeasiblePopulation = new Population();
    public Population feasiblePopulation = new Population();
    public Population noveltyArchive = new Population();

    // Used to keep track of all the feasible Individuals
    public Population feasibleIndividuals = new Population();

    private bool initialisedInfeasiblePop;
    private bool initialisedFeasiblePop;
    private int feasibleIndividualGeneration;

    public Individual infeasibleFittest = new Individual();
    //public Individual feasibleFittest = new Individual();

    public Individual fittestIndividualDiversity = new Individual();
    public Individual fittestIndividualActual = new Individual();

    public float fittestInfeasible = 0;
    public float fittestFeasible = 0;
    public float fitnessInfeasible = 0;
    public float fitnessFeasible = 0;
    public int currentFeasibleIndividual = 0;
    public int currentInfeasibleIndividual = 0;
    public bool minimalCriteria;
    public bool deathPenalty;

    public FI2PopNsGA(bool minimalCriteria, bool deathPenalty, int populationSize, int genomeLength, float mutationRate, bool elitism, CrossoverType crossoverType, int tournamentSize, float evaluationTime, bool testing,
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
            int minutes = Mathf.FloorToInt(Time.time / 60F);
            int seconds = Mathf.FloorToInt(Time.time - minutes * 60);
            time = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Will spawn infeasible levels and evaluate them
            if (!initialisedInfeasiblePop)
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
                    noveltyArchive.Add(Utility.DeepClone(feasiblePopulation.GetFittest()));
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
                Debug.Log("Current run produced " + feasibleIndividualCount + " feasible individuals, with a best fitness score of " +
                fittestIndividualActual.fitness + " and a diversity score of " + fittestIndividualDiversity.fitness + ", generated at generation #" + feasibleIndividualGeneration);
                output += feasibleIndividualCount + ", " + fittestFeasible + ", " + feasibleIndividualGeneration + "\n";

                // Append title to csv
                if (csv.Length == 0)
                {
                    AddDataToResults(string.Format("{0},{1},{2},{3},{4}", "Number of Feasible individuals", "Fittest Individual Fitness", "Fittest Individual Novelty", "Generation of Fittest Individual", "Generation of First Feasible Individual"));
                }
                // Append new line to csv
                AddDataToResults(string.Format("{0},{1},{2},{3},{4}", feasibleIndividualCount, fittestIndividualActual.fitness, fittestIndividualDiversity.fitness, feasibleIndividualGeneration, firstFeasibleGeneration));


                currentTestRun++;
                levelGenerator.ClearScene();
                if (currentTestRun <= testRuns - 1)
                {
                    infeasiblePopulation.individuals.Clear();
                    feasiblePopulation.individuals.Clear();
                    infeasibleFittest = new Individual();
                    fittestIndividualActual = new Individual();
                    fittestIndividualDiversity = new Individual();
                    feasibleIndividualCount = 0;
                    FitnessVisualizerEditor.values.Clear();
                    FitnessVisualizerEditor.values2.Clear();
                    FitnessVisualizerEditor.values3.Clear();
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
                //Individual final = Individual.FromJson(Application.dataPath + "/Levels/" + "gl" + genomeLength + "f" + fittest + ".json");
                levelGenerator.ClearScene();

                if (feasibleIndividualCount > 0)
                {
                    Debug.Log("Clearing and spawning fittest, it has a fitness of: " + fittestIndividualActual.fitness);
                    levelGenerator.DisplayIndividual(fittestIndividualActual);
                }
                else
                {
                    Individual fittest = infeasiblePopulation.GetFittest();
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
            if (cooldown >= evaluationTime + Time.deltaTime)
            {

                connectedComponents = LevelGenerator.graph.CalculateConnectivity();

                GraphEditor.InitRects(genomeLength);


                pop.individuals[currentInfeasibleIndividual].fitness = CalculateConstraintFitness();// (CalculateConstraintFitness() * 0.5f + CalculateKVertexConnectivtyFitness() * 1.5f) / 2;
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
            if (cooldown >= evaluationTime + Time.deltaTime)
            {
                GraphEditor.InitRects(genomeLength);
                connectedComponents = LevelGenerator.graph.CalculateConnectivity();

                //pop.individuals[currentFeasibleIndividual].fitness = CalculateCombinedFitness();
                // Calculate fitness
                float averageDiversity = 0;
                int divisor = 0;
                // Compare current individual to neighbours in the population
                for (int neighbour = 0; neighbour < 2; neighbour++)
                {
                    if (currentFeasibleIndividual < pop.Size() - 1)
                    {
                        //Debug.Log(currentInfeasibleIndividual);
                        averageDiversity += pop.individuals[currentFeasibleIndividual].GetDiversity(pop.individuals[currentFeasibleIndividual + neighbour]);
                        //Debug.Log(pop.individuals[currentIndividual].GetDiversity(pop.individuals[currentIndividual + neighbour]));
                        divisor++;
                    }

                    if (currentFeasibleIndividual > 0)
                    {
                        averageDiversity += pop.individuals[currentFeasibleIndividual].GetDiversity(pop.individuals[currentFeasibleIndividual - neighbour]);
                        divisor++;
                    }
                }
                // Compare current individual with novelty archive
                for (int i = 0; i < noveltyArchive.Size(); i++)
                {
                    averageDiversity += pop.individuals[currentFeasibleIndividual].GetDiversity(noveltyArchive.individuals[i]);
                    divisor++;
                }

                averageDiversity /= divisor;
                pop.individuals[currentFeasibleIndividual].fitness = averageDiversity;

                // MCNS
                if (minimalCriteria)
                {
                    // Penalty if not feasible
                    if (connectedComponents != 1)
                    {
                        if (deathPenalty)
                        {
                            // Death penalty
                            pop.individuals[currentFeasibleIndividual].fitness = 0;
                        }
                        else
                        {
                            // Penalty based on distance from feasibility
                            pop.individuals[currentFeasibleIndividual].fitness *= CalculateConstraintFitness();
                        }
                    }
                }

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
                    fitnessFeasible = pop.individuals[currentFeasibleIndividual].fitness;
                    FitnessVisualizerEditor.values3.Add(fitnessFeasible);
                    float actualFitness = CalculateCombinedFitness();
                    FitnessVisualizerEditor.values.Add(actualFitness);
                    if (fitnessFeasible > fittestFeasible)
                    {
                        feasibleIndividualGeneration = generation;
                        fittestFeasible = fitnessFeasible;
                        fittestIndividualDiversity = Utility.DeepClone(pop.individuals[currentFeasibleIndividual]);
                        fittestIndividualActual = Utility.DeepClone(pop.individuals[currentFeasibleIndividual]);
                        fittestIndividualActual.fitness = actualFitness;
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
