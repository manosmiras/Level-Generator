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
    public GameObject room;
    public GameObject aStar;
    [ReadOnly] public int currentIndividual = 0;
    [ReadOnly] public int generation = 1;
    [ReadOnly] public int fittest;
    [ReadOnly] public int fitness = 0;
    public static int genomeLength = 50;
    public static int populationSize = 75;
    [ReadOnly] public bool initialised;
    // Private properties
    private float cooldown = 0;
    private static int tournamentSize = 5;
    [ReadOnly] public bool displaying = false;
    private Population population = new Population();
    private float positionModifier = 2.5f;
    [ReadOnly] public bool elitism;
    private static int mutationRate = 15;
    // Use this for initialization
    void Start()
    {
        initialised = false;
        GenerateRandomPopulation(populationSize);
        
        generation = 1;
        fittest = int.MaxValue;
        elitism = true;
    }

    // Update is called once per frame
    void Update()
    {
        DisplayPopulation();

        //if (initialised)
        //{
        //    EvolvePopulation();
        //}
        if (initialised)
        {
            generation++;
            //System.out.println("Generation: " + generationCount + " Fittest: " + myPop.getFittest().getFitness());
            population = EvolvePopulation();

            currentIndividual = 0;
            initialised = false;
            
        }
    }

    Population EvolvePopulation()
    {
        Population newPopulation = new Population();

        // Keep our best individual
        if (elitism)
        {
            newPopulation.Add(population.GetFittest());
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
        for (int i = elitismOffset; i < populationSize; i++)
        {
            Individual individual1 = TournamentSelection();
            Individual individual2 = TournamentSelection();
            Individual newIndividual = Crossover(individual1, individual2);
            newPopulation.Add(newIndividual);
        }

        // Mutate population
        for (int i = elitismOffset; i < newPopulation.individuals.Count; i++)
        {
            Mutate(newPopulation.individuals[i]);
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
            Vector2 position = new Vector2(Random.Range(-genomeLength / 2, genomeLength / 2) * positionModifier,
                                           Random.Range(-genomeLength / 2, genomeLength / 2) * positionModifier);
            float rotation = Random.Range(0, 2);
            rotation *= 90f;
            int roomType = Random.Range(0, 4);
            LevelPiece piece = new LevelPiece(position, rotation, (LevelPiece.Type)roomType);
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
            if (cooldown >= .1)
            {
                // Calculate fitness for current individual
                population.individuals[currentIndividual].fitness = overlapPenalty + (genomeLength - connectedCount);
                fitness = population.individuals[currentIndividual].fitness;
                // Closest to 0 is fittest
                if (fitness < fittest)
                {
                    fittest = fitness;
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

    Individual Crossover(Individual individual1, Individual individual2)
    {
        Individual offspring = new Individual();
        // Loop through all design elements
        for (int i = 0; i < individual1.designElements.Count; i++)
        {
            float rand = Random.Range(0.0f, 1.0f);
            // 50% chance to copy design element from individual 1
            if (rand <= 0.5f)
            {
                offspring.designElements.Add(individual1.designElements[i]);
            }
            // 50% chance to copy design element from individual 2
            else
            {
                offspring.designElements.Add(individual2.designElements[i]);
            }
        }
        return offspring;
    }

    Individual TournamentSelection()
    {
        // Create a tournament population
        Population tournamentPopulation = new Population();

        // For each place in the tournament get a random individual
        for (int i = 0; i < tournamentSize; i++)
        {
            int randomId = Random.Range(0, populationSize);
            tournamentPopulation.Add(population.individuals[randomId]);
        }

        Individual fittest = tournamentPopulation.individuals[0];
        // Get the fittest
        for (int i = 0; i < tournamentSize; i++)
        {
            if (fittest.fitness <= tournamentPopulation.individuals[i].fitness)
            {
                fittest = tournamentPopulation.individuals[i];
            }
        }
        return fittest;
    }

    // Mutate an individual
    void Mutate(Individual individual)
    {
        // Loop through genes
        for (int i = 0; i < individual.designElements.Count; i++)
        {
            if (Random.Range(0, 100) <= mutationRate)
            {
                Vector2 position = new Vector2(Random.Range(-genomeLength / 2, genomeLength / 2) * positionModifier,
                                               Random.Range(-genomeLength / 2, genomeLength / 2) * positionModifier);
                float rotation = Random.Range(0, 2);
                rotation *= 90f;
                int roomType = Random.Range(0, 4);
                LevelPiece piece = new LevelPiece(position, rotation, (LevelPiece.Type)roomType);

                individual.designElements[i] = piece;
            }
        }
    }

    void DisplayIndividual(Individual individual)
    {
        int count = 0;
        foreach (LevelPiece piece in individual.designElements)
        {
            count++;
            switch (piece.type)
            {
                case LevelPiece.Type.Cross:
                    GameObject tempCross = Instantiate(cross, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCross.transform.parent = gameObject.transform;
                    tempCross.name += count;
                    tempCross.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.T_Junction:
                    GameObject tempT_Junction = Instantiate(t_junction, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempT_Junction.transform.parent = gameObject.transform;
                    tempT_Junction.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Hall:
                    GameObject tempHall = Instantiate(hall, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempHall.transform.parent = gameObject.transform;
                    tempHall.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Corner:
                    GameObject tempCorner = Instantiate(corner, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempCorner.transform.parent = gameObject.transform;
                    tempCorner.tag = "LevelPiece";
                    break;
                case LevelPiece.Type.Room:
                    GameObject tempRoom = Instantiate(room, new Vector3(piece.position.x, 0, piece.position.y), new Quaternion(0, piece.rotation, 0, 1)) as GameObject;
                    tempRoom.transform.parent = gameObject.transform;
                    tempRoom.tag = "LevelPiece";
                    break;
            }
        }
        // TODO: Fix overlap and disconnected penalties detection to work, maybe make coroutine or async?
        generated = true;
        // Initialise pathfinding
        Instantiate(aStar, new Vector3(), new Quaternion());
    }

    void ClearScene()
    {
        // Destroys A* prefab and any level pieces that were spawned
        Destroy(GameObject.FindGameObjectWithTag("A*"));
        GameObject[] levelPieces = GameObject.FindGameObjectsWithTag("LevelPiece");
        foreach (GameObject levelPiece in levelPieces)
        {
            DestroyImmediate(levelPiece, true);
        }
    }


}