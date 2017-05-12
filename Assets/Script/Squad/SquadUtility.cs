using System;

public static class SquadUtility
{
    static public float CalculateCombatFactor(IUnit attacker)
    {
        float modifier;
        //Apply strength
        if (attacker.a_strenght.currValue <= 4.0f)
        {
            modifier = 1.0f - (3 - attacker.a_strenght.currValue) * 0.1f;
        }
        else
        {
            modifier = 1.0f + (attacker.a_strenght.currValue - 3) * 0.05f;
        }

        //Apply combat skill
        if (attacker.s_combat <= 5.0f)
        {
            modifier *= 1.0f - (5 - attacker.s_combat) * 0.1f;
        }
        else if (attacker.s_combat <= 10.0f)
        {
            modifier *= 1.0f + (attacker.s_combat - 5) * 0.05f;
        }
        else
        {
            modifier *= 1.0f + (attacker.s_combat - 5) * 0.05f + (attacker.s_combat - 10) * 0.10f;
        }

        //Apply combat perks
        foreach (IPerk x in attacker.p_perks)
        {
            if (x.IsSatisfied(PerkCondition.COMBAT, attacker, null))
                modifier *= x.bonus;
        }

        return modifier;
    }

}

