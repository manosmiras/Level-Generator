using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public List<DesignElement> designElements = new List<DesignElement>();
    public int fitness;

    public Individual()
    {
        
    }

    public Individual(List<DesignElement> designElements)
    {
        this.designElements = designElements;
    }
}
