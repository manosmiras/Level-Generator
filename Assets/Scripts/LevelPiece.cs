using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPiece : DesignElement
{
    public enum Type
    {
        Cross = 0,
        T_Junction = 1,
        Hall = 2,
        Corner = 3,
        Room = 4
    }

    public Type type;
    public LevelPiece(Vector2 position, float rotation, Type type)
    {
        this.position = position;
        this.rotation = rotation;
        this.type = type;
    }
}
