using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
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
    // Copy constructor
    public Population(Population pop)
    {
        individuals = new List<Individual>(pop.individuals);
    }
    
    public Population(List<Individual> individuals)
    {
        this.individuals = new List<Individual>(individuals);
    }
    
    public Individual GetFittestIndividual()
    {
        var fittest = individuals[0];
        for (var i = 0; i < individuals.Count; i++)
        {
            if (individuals[i].fitness > fittest.fitness)
            {
                fittest = individuals[i];
            }
        }
        return fittest;
    }

    public Individual GetWeakestIndividual()
    {
        var weakest = individuals[0];
        for (var i = 1; i < individuals.Count; i++)
        {
            if (individuals[i].fitness < weakest.fitness)
            {
                weakest = individuals[i];
            }
        }
        return weakest;
    }

    public int GetWeakestIndex()
    {
        var weakest = individuals[0];
        var index = 0;
        // Get the fittest
        for (var i = 1; i < individuals.Count; i++)
        {
            if (individuals[i].fitness < weakest.fitness)
            {
                weakest = individuals[i];
                index = i;
            }
        }
        return index;
    }

    public int Size()
    {
        return individuals.Count;
    }

    public void Print()
    {
        foreach (var individual in individuals)
        {
            individual.Print();
        }
    }
    public void Sort()
    {
        foreach (var individual in individuals)
        {
            individual.Sort();
        }
    }
}
