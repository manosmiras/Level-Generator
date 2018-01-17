using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population
{
    public List<Individual> individuals = new List<Individual>();

    public void Add(Individual individual)
    {
        individuals.Add(individual);
    }

    // Default Public constructor
    public Population()
    {

    }
    // Public constructor with argument
    public Population(List<Individual> individuals)
    {
        this.individuals = individuals;
    }
    // Gets fittest individual in the population
    public Individual GetFittest()
    {
        Individual fittest = individuals[0];
        // Get the fittest
        for (int i = 0; i < individuals.Count; i++)
        {
            if (fittest.fitness <= individuals[i].fitness)
            {
                fittest = individuals[i];
            }
        }
        return fittest;
    }

    public void Print()
    {
        foreach (Individual individual in individuals)
        {
            individual.Print();
        }
    }
    public void Sort()
    {
        foreach (Individual individual in individuals)
        {
            individual.Sort();
        }
    }
}
