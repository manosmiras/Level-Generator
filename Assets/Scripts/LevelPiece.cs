using UnityEngine;
using System;
[Serializable]
public class LevelPiece : IEquatable<LevelPiece>
{
    public enum Type
    {
        Cross,
        T_Junction,
        Hall,
        Corner,
        Room1,
        Room2,
        Room3,
        Room4,
        Cross_Trap,
        T_Junction_Trap,
        Hall_Trap,
        Corner_Trap,
        Room1_Trap,
        Room2_Trap,
        Room3_Trap,
        Room4_Trap
    }
    
    [SerializeField]
    public Type type;
    [SerializeField]
    public Vector2 position;
    [SerializeField]
    public float rotation;
    public LevelPiece(Vector2 position, float rotation, Type type)
    {
        this.position = position;
        this.rotation = rotation;
        this.type = type;
    }

    public bool Equals(LevelPiece other)
    {
        var otherLP = other;
        return rotation == otherLP.rotation
               && type == otherLP.type;
    }
}
