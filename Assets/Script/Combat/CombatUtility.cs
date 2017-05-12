using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

static public class CombatUtility
{

    /// <summary>
    /// Runs a full combat turn between two squads
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    public static void CombatTurn(Squad attacker, Squad defender)
    {
        //Squads make initial formation
        //TODO: FRONTLINE AND BACKLINE DIFFERENCIATION 
        //FOR NOW: Seperate in 3 groups as equal as possible
        Flank[] flanks;
        FormInitialFlanks(attacker, defender, out flanks);

        //BEGIN LOOP
        //Initiative is rolled
        while(!CombatIsOver(attacker, defender))
        {
            Debug.Log("NEW TURN");
            List<IUnit> turnOrder = TurnOrder(attacker, defender);
            //Units attack from possible targets depending on formation
            for(int i = 0; i < turnOrder.Count; i++)
            {
                IUnit unit = turnOrder[i];
                //Skip if unit is dead
                if (!unit.isAlive || unit.isHarmless)
                    continue;

                //Find in which flank the unit is
                Flank current = null;
                bool isSideA = false;
                for(int j = 0; j < 3 && current == null; j++)
                {
                    if (flanks[j].FindUnit(unit, out isSideA))
                        current = flanks[j];
                }
                //Error case?
                if (current == null)
                {
                    Debug.LogWarning("Current could not be found");
                    continue;
                }

                //Acquire random target
                IUnit target = current.RandomTarget(isSideA);
                //Skip if flank was defeated
                if (target == null)
                    continue;

                //Resolve attack
                if(ResolveAttack(unit, target))
                {
                    //Remove dead soldiers
                    current.FlankMemberDied(target);
                    //turnOrder.Remove(target);
                }
            }

            //Flanks with no more targets join another flank for next round
            RearrangeFlanks(flanks);
        }

        Debug.Log("BATTLE OVER");

    }

    public static float CalculateHitChance(IUnit attacker, IUnit defender)
    {
        //Accuracy is determined by combat skill and agility
        float accuracy = GameMath.CalculateSkillRatio(attacker.s_combat);
        accuracy += GameMath.CalculateAttributeRatio(attacker.a_agility.currValue);
        accuracy = Mathf.Pow(accuracy, 3.75f);

        //Dodge chance is also determined by combat skill and agility
        float dodge = GameMath.CalculateSkillRatio(defender.s_combat);
        dodge += GameMath.CalculateAttributeRatio(defender.a_agility.currValue);
        dodge = Mathf.Pow(dodge, 3.75f);

        //TODO: perks

        float hitChance = accuracy / (accuracy + dodge);

        //Debug.Log("Hit chance - " + hitChance);
        return hitChance;
    }

    /// <summary>
    /// Resolves an attack between two units.
    /// Returns true if the defender has died.
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public static bool ResolveAttack(IUnit attacker, IUnit defender)
    {
        float hitChance = CalculateHitChance(attacker, defender);
        //If it is a hit
        if(Random.Range(0, 1.0f) < hitChance)
        {
            //Calculate combat damage
            float combatDamage = GameMath.CalculateCombatDamage(attacker, defender);

            //Distribute the damage to a random body part?
            float totalSize = 0;
            foreach (BodyPart x in defender.b_parts)
            {
                if(x.wound.gravity != Wound.WoundGravity.REMOVED)
                {
                    totalSize += x.size;
                }
            }

            float randomPlace = Random.Range(0, totalSize);

            BodyPart partHit = null;
            int index = 0;
            while (randomPlace > 0.0f && index < defender.b_parts.Count)
            {
                partHit = defender.b_parts[index];
                randomPlace -= partHit.size;
                index++;
            }

            //Apply damage
            //Debug.Log(attacker.name + " struck " + defender.name);
            DamageBodyPart(defender, partHit, combatDamage);

            //Check if unit is dead
            if (partHit.partFunctions.Contains(BodyPart.BodyPartFunction.VITAL) && partHit.wound.gravity.Equals(Wound.WoundGravity.REMOVED))
            {
                defender.isAlive = false;
                Debug.Log(defender.name + " has died.");
                return true;
            }
            if(defender.CheckIfHarmless())
            {
                Debug.Log(defender.name + " is out of combat. ");
                return true;
            }
        }
        else
        {
            //Debug.Log(attacker.name + " missed " + defender.name);
        }
        return false;
    }

    /// <summary>
    /// Inflict damage to a body part depending on total damage and unit toughness.
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="part"></param>
    /// <param name="damage"></param>
    public static void DamageBodyPart(IUnit defender, BodyPart part, float damage)
    {
        int severity = Mathf.RoundToInt(damage / (defender.a_toughness.currValue));
        int currentWound = (int)part.wound.gravity;
        currentWound += severity;

        if (currentWound > (int)Wound.WoundGravity.REMOVED)
            currentWound = (int)Wound.WoundGravity.REMOVED;

        part.wound.gravity = (Wound.WoundGravity)currentWound;

        Debug.Log(defender.name + " " + part.name + " is now " + part.wound.gravity.ToString() + " (" + severity + ")");

    }

    static void FormInitialFlanks(Squad squadA, Squad squadB, out Flank[] flanks)
    {
        flanks = new Flank[3];
        for (int i = 0; i < 3; i++)
        {
            flanks[i] = new Flank();
        }

        //////////////////////////
        //First squad
        //////////////////////////
        List<IUnit> remaining = new List<IUnit>();
        remaining.AddRange(squadA.members);
        Debug.Log(squadA.members[0] + " - " + remaining[0]);

        //Leaders in the middle
        flanks[1].AddMember(squadA.leader, true);
        remaining.Remove(squadA.leader);

        //Flanks are chosen by preferences if present, else by combat skill
        //TODO: Preferences
        remaining = remaining.OrderByDescending(x => x.s_combat).ToList();

        //Add units in this following order: Left, right, middle
        while(true)
        {
            flanks[0].AddMember(remaining[0], true);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;

            flanks[2].AddMember(remaining[0], true);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;

            flanks[1].AddMember(remaining[0], true);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;
        }

        //////////////////////////
        //Second squad
        //////////////////////////
        remaining.Clear();
        remaining.AddRange(squadB.members);

        //Leaders in the middle
        flanks[1].AddMember(squadB.leader, false);
        remaining.Remove(squadB.leader);

        //Flanks are chosen by preferences if present, else by combat skill
        //TODO: Preferences
        remaining = remaining.OrderByDescending(x => x.s_combat).ToList();

        //Add units in this following order: Left, right, middle
        while (true)
        {
            flanks[0].AddMember(remaining[0], false);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;

            flanks[2].AddMember(remaining[0], false);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;

            flanks[1].AddMember(remaining[0], false);
            remaining.RemoveAt(0);
            if (remaining.Count == 0)
                break;
        }

        Debug.Log("Final flank size " +
            flanks[0].sideA.Count + " - " + flanks[0].sideB.Count +
            flanks[1].sideA.Count + " - " + flanks[1].sideB.Count +
            flanks[2].sideA.Count + " - " + flanks[2].sideB.Count);

    }

    static List<IUnit> TurnOrder(Squad attacker, Squad defender)
    {
        List<IUnit> turnOrder = new List<IUnit>();

        //Each member rolls on his agility + combat skill + quarter leader combat skill 

        foreach (IUnit x in attacker.members)
        {
            if(x.isAlive && !x.isHarmless)
            {
                x.RollInitiative(attacker.leader);
                turnOrder.Add(x);
            }            
        }

        foreach (IUnit x in defender.members)
        {
            if (x.isAlive && !x.isHarmless)
            {
                x.RollInitiative(defender.leader);
                turnOrder.Add(x);
            }
        }

        turnOrder = turnOrder.OrderByDescending(x => x.c_initiative).ToList();

        return turnOrder;
    }

    static bool CombatIsOver(Squad attacker, Squad defender)
    {
        bool attackerStand = false;
        bool defenderStand = false;

        foreach (IUnit x in attacker.members)
        {
            if (x.isAlive && !x.isHarmless)
            {
                attackerStand = true;
                break;
            }
        }

        foreach (IUnit x in defender.members)
        {
            if (x.isAlive && !x.isHarmless)
            {
                defenderStand = true;
                break;
            }
        }

        return !(attackerStand && defenderStand);
    }

    static void RearrangeFlanks(Flank[] flanks)
    {        
        //Check if side flanks are free and join in mid or other side
        //Left
        if(!flanks[0].IsFighting())
        {
            if (!flanks[1].IsEmpty())
                flanks[0].CombineWithFlank(flanks[1]);
            else if (!flanks[2].IsEmpty())
                flanks[0].CombineWithFlank(flanks[2]);
        }
        //Right
        if (!flanks[2].IsFighting())
        {
            if (!flanks[1].IsEmpty())
                flanks[2].CombineWithFlank(flanks[1]);
            else if (!flanks[0].IsEmpty())
                flanks[2].CombineWithFlank(flanks[0]);
        }
        //Mid
        if (!flanks[1].IsFighting())
        {
            if (!flanks[0].IsEmpty())
                flanks[1].CombineWithFlank(flanks[0]);
            else if (!flanks[2].IsEmpty())
                flanks[1].CombineWithFlank(flanks[2]);
        }
    }
}

