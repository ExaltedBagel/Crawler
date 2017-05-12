using System;
using System.Collections.Generic;

public class Relationship
{
    Relationship(IUnit other, float base_appreciation)
    {
        appreciation = base_appreciation;

        //Calculate shit relation to other unit stats
        
    }

    RelationshipTitle relation { get; }
    IUnit other { get; }
    HashSet<IRelationModifier> modifiers { get; }
    float appreciation { get; set; }
    float powerDifference { get; set; }

    public float GetAppreciation()
    {
        float value = appreciation;

        foreach (IRelationModifier x in modifiers)
        {
            value *= x.bonus;
        }

        return value;
    }

    public enum RelationshipTitle
    {
        NEMESIS,
        ENEMY,
        ANNOYED,
        NEUTRAL,
        APPRECIATE,
        FRIEND,
        ALLY,
        BOSS,
        TYRANT,
        LEADER
    }

}


