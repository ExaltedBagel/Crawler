using System;
using System.Collections.Generic;

public class Weapon : IItem
{
    public bool isNatural { get; }
    public Range range { get; }
    public float minDmg { get; set; }
    public float maxDmg { get; set; }
    public List<PerkCondition> perkTriggers { get; set; }

    public Weapon(string name, float cost, float minDmg, float maxDmg, bool isNatural, Range range = Range.MELEE) : base(name, cost)
    {
        this.range = range;
        this.isNatural = isNatural;
        this.minDmg = minDmg;
        this.maxDmg = maxDmg;
        perkTriggers = new List<PerkCondition>();
    }

    public enum Range
    {
        MELEE,
        RANGED
    }
}
