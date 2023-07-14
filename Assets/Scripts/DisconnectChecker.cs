using System.Collections.Generic;
using UnityEngine;

public class DisconnectChecker : MonoBehaviour
{
    private List<string> _otherCollisions = new();
    public bool didCollide = false;
    private void Start()
    {
        didCollide = false;
    }
    void OnTriggerEnter(Collider other)
    {

        // If other parent exists
        if (other.transform.parent)
        {
            // Get parent name
            string otherColliderName = other.transform.parent.name;

            // Checks for collisions of room entries
            if (gameObject.tag == "Entry_Colliders" && other.tag == "Entry_Colliders" && !_otherCollisions.Contains(otherColliderName))
            {

                _otherCollisions.Add(otherColliderName);

                // Add child piece to current piece
                LevelGenerator.graph.Get(transform.parent.name).children.Add(LevelGenerator.graph.Get(otherColliderName));

                LevelGenerator.connectedCount++;

                didCollide = true;
            }
        }

    }
}