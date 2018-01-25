using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public abstract class DesignElement : IEquatable<DesignElement>
{
    public Vector2 position;
    public float rotation;

    public abstract bool Equals(DesignElement other);
}
