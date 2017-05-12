using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Attribute
{
    public Attribute(float baseValue, int baseXp)
    {
        this.baseValue = baseValue;
        this.currValue = baseValue;
    }

    public float baseValue { get; set; }
    public float currValue { get; set; }
    float experience;
    public void ResetValue() { currValue = baseValue; }
}
