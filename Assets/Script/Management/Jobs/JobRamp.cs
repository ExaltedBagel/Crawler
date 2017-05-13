using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobRamp : IJob
{
    int direction;

    public JobRamp(int x, int z, int level, int direction) : base(x, z, level)
    {
        this.direction = direction;
        progress = 10;
        jobName = "Dig";
    }

    public override void AssignUnit(IUnit unit)
    {
        unit.m_taskQueue.Enqueue(new JobMoveTo(x, z, level));
        unit.m_taskQueue.Enqueue(this);
    }


    public override void FreeUnit(IUnit unit)
    {

    }

    public override void OnFinished()
    {
        base.OnFinished();

        // Change the tile
        MapGenerator map = GameObject.Find("Map").GetComponent<MapGenerator>();
        switch (direction)
        {
            case 0:
                map.Floors[level][x, z].Content = TileContent.SLOPE_D_L;
                map.Floors[level + 1][x - 1, z].Content = TileContent.SLOPE_U_R;
                map.Floors[level + 1][x, z].Content = TileContent.UNDERSLOPE;
                break;
            case 1:
                map.Floors[level][x, z].Content = TileContent.SLOPE_D_U;
                map.Floors[level + 1][x, z + 1].Content = TileContent.SLOPE_U_D;
                map.Floors[level + 1][x, z].Content = TileContent.UNDERSLOPE;
                break;
            case 2:
                map.Floors[level][x, z].Content = TileContent.SLOPE_D_R;
                map.Floors[level + 1][x + 1, z].Content = TileContent.SLOPE_U_L;
                map.Floors[level + 1][x, z].Content = TileContent.UNDERSLOPE;
                break;
            case 3:
                map.Floors[level][x, z].Content = TileContent.SLOPE_D_D;
                map.Floors[level + 1][x, z - 1].Content = TileContent.SLOPE_U_U;
                map.Floors[level + 1][x, z].Content = TileContent.UNDERSLOPE;
                break;
            default:
                break;
        }
        var tool = GameObject.Find("MapBuild").GetComponent<BuildToolManager>();

        tool.RebuildFloor(x - 1, x + 1, z - 1, z + 1, level);
        tool.RebuildFloor(x - 1, x + 1, z - 1, z + 1, level + 1);
    }

    public override void OnStart(IUnit unitStarting)
    {

    }

    public override void OnUpdate(IUnit unit)
    {
        ProgressBy(Mathf.RoundToInt(unit.a_strenght.currValue));
        unit.PlayActionAnimation();
        if (IsDone())
        {
            unit.JobFinished();
        }
    }
}
