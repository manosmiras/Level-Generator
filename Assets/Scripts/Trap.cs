using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : DesignElement {
    public enum Type
    {
        Spike = 0,
    }

    public Type type;

    public Trap(Vector2 position, float rotation, Type type)
    {
        this.position = position;
        this.rotation = rotation;
        this.type = type;
    }

    public override bool Equals(DesignElement other)
    {
        throw new System.NotImplementedException();
    }
}
