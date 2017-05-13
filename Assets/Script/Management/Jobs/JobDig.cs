using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobDig : IJob
{
    public JobDig(int x, int z, int level) : base(x,z, level, false)
    {
        progress = 10;
        jobName = "Dig";
    }

    public override void AssignUnit(IUnit unit)
    {
        base.AssignUnit(unit);
        unit.m_taskQueue.Enqueue(new JobMoveTo(x, z, level));
        unit.m_taskQueue.Enqueue(this);
    }

    public override void FreeUnit(IUnit unit)
    {
        base.FreeUnit(unit);
    }

    public override void OnFinished()
    {
        base.OnFinished();
        // Change the tile
        MapGenerator map = GameObject.Find("Map").GetComponent<MapGenerator>();
        map.Floors[level][x,z].Content = TileContent.FLOOR;
        var tool = GameObject.Find("MapBuild").GetComponent<BuildToolManager>();
        tool.RebuildFloor(x-1, x+1, z-1, z+1);
    }

    public override void OnStart(IUnit unitStarting)
    {
        unitStarting.m_state = IUnit.State.WORKING;
    }

    public override void OnUpdate(IUnit unit)
    {
        ProgressBy(Mathf.RoundToInt(unit.a_strenght.currValue));
        unit.PlayActionAnimation();
        if(IsDone())
        {
            unit.JobFinished();
        }
    }


}
