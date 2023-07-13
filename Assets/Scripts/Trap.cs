using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class Trap : DesignElement {

    public Trap(Vector2 position, float rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public override bool Equals(DesignElement other)
    {
        throw new System.NotImplementedException();
    }
}
