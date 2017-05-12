using System;
using System.Collections.Generic;

public abstract class IItem
{
    public string name;
    public float cost;
    public IItem(string name, float cost)
    {
        this.name = name;
        this.cost = cost;
    }
}

