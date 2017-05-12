using System;

public class PerkVeteran : IPerk
{
    public PerkVeteran()
    {
        bonus = 1.2f;
        perkName = "Combat Veteran";
        description = "This unit has seen more than a fight! Gains 20% to it's combat score when in combat or raiding.";
    }

    public override bool IsSatisfied(PerkCondition action, IUnit self, IUnit target)
    {
        return (action.Equals(PerkCondition.COMBAT)    || 
                action.Equals(PerkCondition.RAID)      );
    }
}
