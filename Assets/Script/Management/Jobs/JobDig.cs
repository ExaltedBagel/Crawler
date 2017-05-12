using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobDig : IJob
{

    public JobDig(int x, int z, int level) : base(x,z, level)
    {
        progress = 10;
        jobName = "Dig";
    }

    public override void AssignUnit(IUnit unit)
    {
        //TODO Condition
        if(!unit.HasAJob())
        {
            assignedUnits.Add(unit);
            unit.m_currentJob = this;
            base.AssignUnit(unit);
            OnStart(unit);
        }
    }

    public override void FreeUnit(IUnit unit)
    {
        unit.ClearJob();
        assignedUnits.Remove(unit);
    }

    public override void OnFinished()
    {
        state = State.DONE;
        foreach(IUnit unit in assignedUnits)
        {
            unit.ClearJob();
        }
        assignedUnits.Clear();
        //Change the tile
        MapGenerator map = GameObject.Find("Map").GetComponent<MapGenerator>();
        map.Floors[level][x,z].Content = TileContent.FLOOR;
        var tool = GameObject.Find("MapBuild").GetComponent<BuildToolManager>();

        tool.RebuildFloor(x-1, x+1, z-1, z+1);

    }

    public override void OnStart(IUnit unitStarting)
    {
        //Head to the destination and set the stopping distance accordingly
        unitStarting.SetDestination(this.GetPosition(), IUnit.StartTheJob);
        state = State.PENDING;
    }


}
