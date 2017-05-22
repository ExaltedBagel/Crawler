using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSleep : IJob
{
    Bed bed;

    public JobSleep(int x, int z, int level, Bed bed) : base(x, z, level, true)
    {
        this.bed = bed;
        progress = 25;
        jobName = "Sleep";
    }

    public override void AssignUnit(IUnit unit)
    {
        base.AssignUnit(unit);
        if (unit.m_Bed != null)
        {
            unit.m_taskQueue.Enqueue(new JobMoveTo(unit.m_Bed.transform.position, true));
        }
        unit.m_taskQueue.Enqueue(this);
    }

    public override void FreeUnit(IUnit unit)
    {
        base.FreeUnit(unit);
    }

    public override void OnFinished(IUnit unit)
    {
        base.OnFinished(unit);
        unit.WakeUp();
        Debug.Log("Goblin woke up!");
    }

    public override void OnStart(IUnit unitStarting)
    {
        unitStarting.FallAsleep();
    }

    public override void OnUpdate(IUnit unit)
    {
        progress--;
        if (IsDone())
        {
            unit.JobFinished();
        }
    }


}
