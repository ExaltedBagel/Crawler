using System;
using System.Collections.Generic;

public class Armor : IItem
{
    public bool isNatural { get; }
    public float protection { get; set; }
    public List<PerkCondition> perkTriggers { get; set; }
    public BodyPart.BodyPartType type { get; }

    public Armor(string name, float cost, float protection, BodyPart.BodyPartType type, bool isNatural = false) : base(name, cost)
    {
        this.type = type;
        this.isNatural = isNatural;
        this.protection = protection;
        perkTriggers = new List<PerkCondition>();
    }
}
