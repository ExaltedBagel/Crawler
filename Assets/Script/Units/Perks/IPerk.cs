using System;
using System.Collections.Generic;

/// <summary>
/// Interface that defines the basic elements of a perk
/// </summary>
public abstract class IPerk
{
    public string perkName { get; set; }
    public string description { get; set; }
    public float bonus { get; set; }
    abstract public bool IsSatisfied(PerkCondition action, IUnit self, IUnit target);
}

