using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectChecker : MonoBehaviour
{
    List<string> otherCollisions = new List<string>();
    void OnTriggerEnter(Collider other)
    {
        //// Checks for collisions of room entries
        //if (gameObject.name.Equals("Entry_Colliders") && other.name.Equals("Entry_Colliders"))
        //{
        //    //Debug.Log("Checking: " + transform.parent.name);
        //    // Check if already added
        //    if (!LevelGenerator.connected.Contains(transform.parent.name))
        //    {
        //        LevelGenerator.connected.Add(transform.parent.name);
        //        LevelGenerator.connectedCount++;
        //        //Debug.Log("Number of rooms connected: " + LevelGenerator.connectedCount);
        //    }
        //}

        // If parent exists
        if (other.transform.parent)
        {
            // Get parent name
            string otherColliderName = other.transform.parent.name;

            // Checks for collisions of room entries
            if (gameObject.name.Equals("Entry_Colliders") && other.name.Equals("Entry_Colliders") && !otherCollisions.Contains(otherColliderName))
            {
                //Debug.Log("name: " + other.name + ", go name " + transform.parent.name);
                otherCollisions.Add(otherColliderName);

                // Check if the other piece already contains this piece as a child, if so, don't add
                //if (!Graph.Get(otherColliderName).children.Contains(Graph.Get(transform.parent.name)))
                //{
                    // Add child piece to current piece
                    LevelGenerator.graph.Get(transform.parent.name).children.Add(LevelGenerator.graph.Get(otherColliderName));
                //}

                LevelGenerator.connectedCount++;
                //TextMesh tm = gameObject.GetComponentInChildren<TextMesh>();
                //tm.text = otherCollisions.Count.ToString();
            }
        }
    }
}
