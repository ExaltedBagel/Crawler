using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IJob {

    protected int level;
    public int Level { get { return level; } }
    protected int x;
    protected int z;
    protected string jobName;
    public State state { get; set; }
    public int progress;
    public bool Personnal { get; }
    public HashSet<IJob> AdjacentJobs { get; set; }
    public HashSet<IUnit> assignedUnits { get; set; }
    protected virtual int Capacity { get { return 1; } }

    public IJob(int x, int z, int level, bool personnal = false)
    {
        state = State.PENDING;
        this.x = x;
        this.z = z;
        this.level = level;
        Personnal = personnal;
        assignedUnits = new HashSet<IUnit>();
        AdjacentJobs = new HashSet<IJob>();
    }

    public IJob(Vector3 position, bool personnal = false)
    {
        state = State.PENDING;
        this.x = Mathf.RoundToInt(position.x);
        this.z = Mathf.RoundToInt(position.z);
        this.level = Mathf.RoundToInt(-position.y/1.25f);
        Personnal = personnal;
        assignedUnits = new HashSet<IUnit>();
        AdjacentJobs = new HashSet<IJob>();
    }

    public bool CanAssignUnit()
    {
        return assignedUnits.Count < Capacity;
    }

    public virtual void AssignUnit(IUnit unit)
    {
        assignedUnits.Add(unit);
    }
    public virtual void FreeUnit(IUnit unit)
    {
        assignedUnits.Remove(unit);
    }
    public abstract void OnStart(IUnit unit);
    public abstract void OnUpdate(IUnit unit);
    public virtual void OnFinished()
    {
        state = State.DONE;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x, level * -1.25f, z);
    }

    public bool IsInRange(Vector3 pos)
    {
        int posX = Mathf.RoundToInt(pos.x);
        int posZ = Mathf.RoundToInt(pos.z);
        int uLevel = Mathf.RoundToInt(-pos.y / 1.25f);

        if (uLevel == level)
        {
            if (posX == x || posX == x - 1 || posX == x + 1)
            {
                if (posZ == z || posZ == z - 1 || posZ == z + 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public virtual bool IsDone()
    {
        return progress <= 0;
    }

    public virtual void ProgressBy(int value)
    {
        progress -= value;
    }

    public IJob GetAdjacentJob()
    {
        foreach (IJob x in AdjacentJobs)
        {
            if (!x.state.Equals(State.DONE))
            {
                if (Tile.IsTileAccessible(x.GetPosition()))
                    return x;
            }
        }
        return null;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
            return false;
        IJob j = obj as IJob;
        if ((System.Object)j == null)
            return false;
        return level == j.level && x == j.x && z == j.z && jobName.Equals(j.jobName);
    }
    public bool Equals(IJob j)
    {
        if ((object)j == null)
            return false;
        return level == j.level && x == j.x && z == j.z && jobName.Equals(j.jobName);
    }

    public enum State
    {
        PENDING,
        PROGRESS,
        DONE,
        UNREACHABLE
    }
}
