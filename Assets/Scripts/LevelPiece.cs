using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class LevelPiece : DesignElement
{
    [SerializeField]
    public enum Type
    {
        Cross = 0,
        T_Junction = 1,
        //Hall = 2,
        Corner = 2,
        Room1 = 3,
        Room2 = 4,
        Room3 = 5

    }
    [SerializeField]
    public Type type;
    public LevelPiece(Vector2 position, float rotation, Type type)
    {
        this.position = position;
        this.rotation = rotation;
        this.type = type;
    }

    public override bool Equals(DesignElement other)
    {
        LevelPiece otherLP = (LevelPiece)other;
        return (position == otherLP.position
            && rotation == otherLP.rotation
            && type == otherLP.type);
    }
}
