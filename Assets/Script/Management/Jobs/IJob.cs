using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IJob {

    protected int level;
    protected int x;
    protected int z;
    protected string jobName;
    public State state { get; set; }
    public Square lastSquareTried;
    public int progress;

    public HashSet<IUnit> assignedUnits { get; set; }

    public IJob(int x, int z, int level)
    {
        state = State.PENDING;
        lastSquareTried = Square.CENTER;
        this.x = x;
        this.z = z;
        this.level = level;
        assignedUnits = new HashSet<IUnit>();
    }

    public virtual void AssignUnit(IUnit unit)
    {
        unit.CancelInvoke("LookForAJob");
    }
    public abstract void FreeUnit(IUnit unit);
    public abstract void OnStart(IUnit unit);
    public abstract void OnFinished();

    public Vector3 GetPosition()
    {
        Debug.Log("GETTING POSITION");
        return new Vector3(x, level * -1.25f, z);
    }

    public bool IsInRange(Vector3 pos)
    {
        int posX = Mathf.RoundToInt(pos.x);
        int posZ = Mathf.RoundToInt(pos.z);
        int uLevel = Mathf.RoundToInt(pos.y / 1.25f);

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

    public enum Square
    {
        CENTER,
        LEFT,
        TOP,
        RIGHT,
        DOWN,
        UNREACHABLE
    }

}
