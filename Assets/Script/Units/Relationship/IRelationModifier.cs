using System;

public abstract class IRelationModifier
{
    string modifierName { get; set; }
    string description { get; set; }
    public float bonus { get; set; }
    float expiration { get; set; }

    public abstract bool IsExpired(float time);
}
