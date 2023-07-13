using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        // Checks for overlapping rooms/halls
        if (gameObject.name.Contains("Walkable") && other.name.Contains("Walkable"))
        {
            // Check if already added
            if (!LevelGenerator.overlapping.Contains(transform.parent.name))
            {
                // Get colliders
                var colliders = GetComponents<Collider>();
                
                // Check all colliders
                for (var i = 0; i < colliders.Length; i++)
                {
                    // Check for intersection
                    if (colliders[i].bounds.Intersects(other.bounds))
                    {
                        LevelGenerator.overlapping.Add(transform.parent.name);
                        LevelGenerator.overlapPenalty++;
                    }
                }
            }
        }
    }
}