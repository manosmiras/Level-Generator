using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour {
    void OnTriggerEnter()
    {

        Debug.Log(gameObject.name + " is colliding with something ");
        if (!LevelGenerator.collisions.Contains(gameObject.name))
        {
            LevelGenerator.collisions.Add(gameObject.name);
            LevelGenerator.overlapPenalty++;
            
        }
        //print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        //print("Their relative velocity is " + collisionInfo.relativeVelocity);
    }
}
