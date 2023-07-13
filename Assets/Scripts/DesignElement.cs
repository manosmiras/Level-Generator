using UnityEngine;
using System;
[Serializable]
public abstract class DesignElement : IEquatable<DesignElement>
{
    [SerializeField]
    public Vector2 position;
    [SerializeField]
    public float rotation;

    public abstract bool Equals(DesignElement other);
}
