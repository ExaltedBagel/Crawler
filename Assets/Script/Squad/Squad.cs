using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A squad is a group of unit acting together. They have a leader which influence their general performance.
/// A squad is used to simplify actions during large scale battles, and also is a driving factor to decision making.
/// </summary>
public class Squad
{
    public string name { get; set; }
    public IUnit leader { get; set; }
    public List<IUnit> members { get; set; }
    float squadPower;
    float squadMoral;
    
    public Squad(IUnit leader)
    {
        name = "";
        this.leader = leader;
        members = new List<IUnit>();
        members.Add(leader);
        CalculateSquadPower();
        //Debug.Log(members.Count + " - " + squadPower);
    }

    public void AddMember(IUnit member)
    {
        members.Add(member);
        CalculateSquadPower();
        //Debug.Log(members.Count + " - " + squadPower);
    }

    /// <summary>
    /// Approximates squad performance depending on individual power and leader influence
    /// </summary>
    public void CalculateSquadPower()
    {
        float totalPower = 0.0f;
        float cohesion = 0.0f;

        foreach (IUnit unit in members)
        {
            //Calculate individual combat skill -- Does not look at perks right now
            totalPower += GameMath.CalculateAttributeRatio(unit.a_strenght.currValue);
            totalPower += GameMath.CalculateSkillRatio(unit.s_combat + (leader.s_combat / 5));
            //totalPower += (SquadUtility.CalculateCombatFactor(unit));
            //Calculate cohesion
            float tempCohesion = 0.0f;
            foreach (IUnit rel in members)
            {
                if (rel.Equals(unit))
                    continue;
                else
                {
                    Relationship r;
                    if (unit.r_relationships.TryGetValue(rel, out r))
                        tempCohesion += r.GetAppreciation();
                }
            }
            tempCohesion /= members.Count;
            cohesion += tempCohesion;
        }

        //Calculate the total
        //TODO - Cohesion does not work for now
        squadPower = totalPower * 1.0f /*cohesion*/ * GameMath.CalculateSkillRatio(leader.s_social) * CalculateSquadSizeAdjustment();
        
    }

    public float CalculateSquadSizeAdjustment()
    {
        //The leader may lead up to his leadership score with no penalty
        if (leader.s_social >= (members.Count - 1))
            return 1.0f;
        //If he has too many units, we must adjust his efficiency as a we add more units
        else
        {
            float difference = members.Count - leader.s_social + 1;
            return (float)Math.Pow(0.95f, difference);
        }
    }

}

