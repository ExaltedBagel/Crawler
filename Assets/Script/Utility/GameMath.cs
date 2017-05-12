using System;
using UnityEngine;
using Random = UnityEngine.Random;



static public class GameMath
{
    /// <summary>
    /// Returns xp required to reach a certain level depending on the base value provided.
    /// A lower base value represents more affinity with the attribute.
    /// </summary>
    /// <param name="baseValue"></param>
    /// <param name="levelWanted"></param>
    /// <returns></returns>
    static public int CalculateNextLevelExp(int baseValue, int levelWanted)
    {
        int value = baseValue;

        for(int i = 1; i < levelWanted; i++)
        {
            //int previousLevel = value;
            value = (int)(value * 1.75f);
        }

        Debug.Log(levelWanted + " - " + value);
        return value;
    }

    static float DamageReductionArmor(float value)
    {
        value /= (value + 40);
        return value;
    }

    /// <summary>
    /// Calculate total combat damage
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    static public float CalculateCombatDamage(IUnit attacker, IUnit defender)
    {
        //Unit find its best weapon
        Weapon weapon = ChooseAttackOption(attacker, defender);

        //A null weapon represents a creature unable to fight
        if (weapon == null)
        {
            attacker.isHarmless = true;
            return 0.0f;
        }

        //float min, max;
        //part.GetBodyMinMax(out min, out max);
        //Roll random value of damage
        float value = Random.Range(weapon.minDmg, weapon.maxDmg);
        float weaponPerks = CalculateWeaponRatio(attacker, defender, weapon);

        //Calculate ratios based on the individual combat skills
        float modifierOffence = CalculateOffensiveRatio(attacker, defender);
        float modifierDefence = CalculateDefensiveRatio(attacker, defender);
        float effectiveArmor = CalculateEffectiveArmor(attacker, defender);
        float armorDamageAdjustment = 1.0f - DamageReductionArmor(effectiveArmor);

        return value * weaponPerks * Mathf.Pow(modifierOffence/modifierDefence, 0.60f) * armorDamageAdjustment;
    }

    static public Weapon ChooseAttackOption(IUnit attacker, IUnit defender)
    {
        //Simple solution: keep the one with best average dmg disregarding the perks
        BodyPart bestOption = attacker.b_parts[0];
        float bestValue = attacker.b_parts[0].BodyPartAtkValue();
        
        foreach (BodyPart x in attacker.b_parts)
        {
            //Get best
            if (bestValue < x.BodyPartAtkValue())
            {
                bestOption = x;
                bestValue = x.BodyPartAtkValue();
            }
        }

        if (bestValue < 0.01f)
            return null;
        if (bestOption.heldItem == null)
        {
            //Debug.Log("Attack option retained is natural attack");
            return bestOption.naturalAttack;
        }
        else
        {
            //Debug.Log("Attack option retained is " + bestOption.heldItem.name);
            return (Weapon)bestOption.heldItem;
        }
    }

    static float CalculateOffensiveRatio(IUnit attacker, IUnit defender)
    {
        //Calculate Attribute ratio
        float modifier = CalculateAttributeRatio(attacker.a_strenght.currValue);

        //Calculate skill ratio
        modifier *= CalculateSkillRatio(attacker.s_combat);

        //Apply combat perks
        foreach (IPerk x in attacker.p_perks)
        {
            if (x.IsSatisfied(PerkCondition.COMBAT, attacker, defender))
                modifier *= x.bonus;
        }

        return modifier;
    }

    static float CalculateDefensiveRatio(IUnit attacker, IUnit defender)
    {
        //Calculate Attribute ratio
        float modifier = CalculateAttributeRatio(defender.a_toughness.currValue);

        //Calculate skill ratio
        modifier *= CalculateSkillRatio(defender.s_combat);

        //Apply combat perks
        foreach (IPerk x in defender.p_perks)
        {
            if (x.IsSatisfied(PerkCondition.COMBAT, defender, attacker))
                modifier *= x.bonus;
        }

        return modifier;
    }

    static float CalculateEffectiveArmor(IUnit attacker, IUnit defender)
    {
        float value = 0.0f;
        float size = 0.0f;
        foreach (BodyPart x in defender.b_parts)
        {
            value += (x.GetTotalArmor() * x.size);
            size += x.size;
        }

        //Apply to armor of the defender
        value /= size;
        //Debug.Log("Armor " + value);

        return value;
    }

    static float CalculateWeaponRatio(IUnit attacker, IUnit defender, Weapon weapon)
    {
        float modifier = 1.0f;
        //Apply combat perks
        foreach (IPerk x in attacker.p_perks)
        {
            foreach (PerkCondition y in weapon.perkTriggers)
            {
                if (x.IsSatisfied(y, attacker, defender))
                {
                    modifier *= x.bonus;
                }
            }
            
        }

        return modifier;
    }

    /// <summary>
    /// Return the ratio of modification of an attribute.
    /// Base: 5 = 0%
    /// Under 5: -5% per point under.
    /// 5 to 10: +5% per point.
    /// Over 10: +10% per point.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    static public float CalculateSkillRatio(float skill)
    {
        float modifier;

        if (skill <= 5.0f)
        {
            modifier = (1.0f - (5 - skill) * 0.1f);
        }
        else if (skill <= 10.0f)
        {
            modifier = (1.0f + (skill - 5) * 0.05f);
        }
        else
        {
            modifier = (1.0f + (skill - 5) * 0.05f + (skill - 10) * 0.10f);
        }

        return modifier;
    }

    /// <summary>
    /// Return the ratio of modification of an attribute.
    /// Base: 3 = 0%
    /// Under 3: -5% per point under.
    /// Over 3: +5% per point.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    static public float CalculateAttributeRatio(float attribute)
    {
        float modifier;
        //Apply strength
        if (attribute <= 4.0f)
        {
            modifier = 1.0f - (3 - attribute) * 0.1f;
        }
        else
        {
            modifier = 1.0f + (attribute - 3) * 0.05f;
        }

        return modifier;
    }

}

