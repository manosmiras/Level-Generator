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
        this.individuals = new List<Individual>(pop.individuals);
    }
    // Public constructor with argument
    public Population(List<Individual> individuals)
    {
        this.individuals = new List<Individual>(individuals);
    }
    // Gets fittest individual in the population
    public Individual GetFittest()
    {
        Individual fittest = individuals[0];
        // Get the fittest
        for (int i = 1; i < individuals.Count; i++)
        {
            if (individuals[i].fitness > fittest.fitness)
            {
                fittest = individuals[i];
            }
        }
        return fittest;
    }

    //public Population DeepCopy()
    //{
    //    Population other = (Population)this.MemberwiseClone();
    //    other.individuals = new List<Individual>(this.individuals);
    //    return other;
    //}

    public int Size()
    {
        return this.individuals.Count;
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
