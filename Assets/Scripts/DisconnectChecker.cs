using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectChecker : MonoBehaviour
{
    int count = 0;
    void OnTriggerEnter(Collider other)
    {
        // Checks for collisions of room entries
        if (gameObject.name.Equals("Entry_Colliders") && other.name.Equals("Entry_Colliders"))
        {
            //Debug.Log("Checking: " + transform.parent.name);
            // Check if already added
            if (!LevelGenerator.connected.Contains(transform.parent.name))
            {
                LevelGenerator.connected.Add(transform.parent.name);
                LevelGenerator.connectedCount++;
                //Debug.Log("Number of rooms connected: " + LevelGenerator.connectedCount);
            }
        }
    }
}
