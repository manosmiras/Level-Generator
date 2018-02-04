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
        Hall = 2,
        Corner = 3,
        Room1 = 4,
        Room2 = 5,
        Room3 = 6

    }
    [SerializeField]
    public Type type;
    //public List<Vector2> trapPositions;
    public LevelPiece(Vector2 position, float rotation, Type type)
    {
        this.position = position;
        this.rotation = rotation;
        this.type = type;

        //switch (type)
        //{
        //    // Issues with rotation?
        //    case Type.Cross:
        //        trapPositions.Add(new Vector2(position.x, position.y));
        //        trapPositions.Add(new Vector2(position.x, position.y + 5));
        //        trapPositions.Add(new Vector2(position.x, position.y - 5));
        //        trapPositions.Add(new Vector2(position.x + 5, position.y));
        //        trapPositions.Add(new Vector2(position.x - 5, position.y));
        //        break;
        //    case Type.T_Junction:
        //        trapPositions.Add(new Vector2(position.x, position.y));
        //        trapPositions.Add(new Vector2(position.x, position.y + 5));
        //        trapPositions.Add(new Vector2(position.x + 5, position.y));
        //        trapPositions.Add(new Vector2(position.x - 5, position.y));
        //        break;
        //    case Type.Hall:
        //        trapPositions.Add(new Vector2(position.x, position.y));
        //        trapPositions.Add(new Vector2(position.x, position.y + 5));
        //        trapPositions.Add(new Vector2(position.x, position.y - 5));
        //        break;
        //    case Type.Corner:
        //        trapPositions.Add(new Vector2(position.x, position.y));
        //        trapPositions.Add(new Vector2(position.x, position.y + 5));
        //        trapPositions.Add(new Vector2(position.x, position.y - 5));
        //        break;
        //}
    }

    public override bool Equals(DesignElement other)
    {
        LevelPiece otherLP = (LevelPiece)other;
        return (//position == otherLP.position
            rotation == otherLP.rotation
            && type == otherLP.type);
    }
}
