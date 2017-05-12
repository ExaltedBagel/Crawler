using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Flank
{
    public HashSet<IUnit> sideA { get; set; }
    public HashSet<IUnit> sideB { get; set; }
    public State flankState;

    public Flank()
    {
        sideA = new HashSet<IUnit>();
        sideB = new HashSet<IUnit>();
    }

    /// <summary>
    /// Gets a random target from the enemies.
    /// </summary>
    /// <returns></returns>
    public IUnit RandomTarget(bool isSideA)
    {
        HashSet<IUnit>.Enumerator it;
        int size;
        if(isSideA)
        {
            it = sideB.GetEnumerator();
            size = sideB.Count;
        }
        else
        {
            it = sideA.GetEnumerator();
            size = sideA.Count;
        }

        for (int i = Random.Range(0, size); i >= 0; i--)
            it.MoveNext();

        return it.Current;
    }

    /// <summary>
    /// Removes a flank member from the set
    /// </summary>
    /// <param name="member"></param>
    public void FlankMemberDied(IUnit member)
    {
        sideA.Remove(member);
        sideB.Remove(member);
    }

    public bool IsEmpty()
    {
        return (sideA.Count == 0 && sideB.Count == 0);
    }

    public bool IsFighting()
    {
        return (sideA.Count > 0 && sideB.Count > 0);
    }

    public bool HasEnemies(bool isSideA)
    {
        if (isSideA)
            return sideB.Count > 0;
        else
            return sideA.Count > 0;
    }

    /// <summary>
    /// Combine the whole flank with an ally and removes all units from current flank
    /// </summary>
    /// <param name="other"></param>
    public void CombineWithFlank(Flank other)
    {
        other.sideA.UnionWith(sideA);
        sideA.Clear();
        other.sideB.UnionWith(sideB);
        sideB.Clear();
    }

    public void AddMember(IUnit ally, bool isSideA)
    {
        if (isSideA)
            sideA.Add(ally);
        else
            sideB.Add(ally);
    }

    public bool FindUnit(IUnit unit, out bool isSideA)
    {

        if(sideA.Contains(unit))
        {
            isSideA = true;
            return true;
        }
        else if (sideB.Contains(unit))
        {
            isSideA = false;
            return true;
        }

        isSideA = true;
        //Debug.Log("Unit not found in squad " + unit.name);
        return false;
    }



    public enum State
    {
        FIGHTING,
        DEFEATED,
        WON,
        MERGED
    }
}
