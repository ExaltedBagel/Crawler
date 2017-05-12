using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class SpearProficiency : IPerk
{
    public SpearProficiency()
    {
        bonus = 1.2f;
        perkName = "Spear Proficiency";
        description = "This unit is proficient with a spear! Gains 20% to it's spear min/max damage.";
    }

    public override bool IsSatisfied(PerkCondition action, IUnit self, IUnit target)
    {
        return action.Equals(PerkCondition.SPEAR);
    }
}

