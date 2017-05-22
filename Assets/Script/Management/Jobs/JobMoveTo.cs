using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public class JobMoveTo : IJob
{
    private bool _OnlyCenter;
    private Vector3 _FinalDestination;
    public JobMoveTo(int x, int z, int level, bool OnlyCenter = false) : base(x,z, level, true)
    {
        _OnlyCenter = OnlyCenter;
        jobName = "Walking";
    }

    public JobMoveTo(Vector3 position, bool OnlyCenter = false) : base(position, true)
    {
        _OnlyCenter = OnlyCenter;
        jobName = "Walking";
    }

    public override void AssignUnit(IUnit unit)
    {
        unit.m_taskQueue.Enqueue(this);
    }

    public override void FreeUnit(IUnit unit)
    {

    }

    public override void OnFinished(IUnit unit)
    {
        base.OnFinished(unit);
    }

    public override void OnStart(IUnit unit)
    {
        unit.m_state = IUnit.State.MOVING;
    }

    public override void OnUpdate(IUnit unit)
    {
        if(unit.m_navAgent.hasPath)
        {
            switch (unit.m_navAgent.pathStatus)
            {
                case NavMeshPathStatus.PathComplete:
                    if(UnitHasReachedDestination(unit))
                    {
                        unit.JobFinished();
                    }
                    break;
                // If we reach this point, unit used to have a valid path. Try to get another one. Cancellation will be handled in called method
                case NavMeshPathStatus.PathPartial:
                case NavMeshPathStatus.PathInvalid:
                    GeneratePath(unit);
                    break;
                default:
                    break;
            }
        }
        else
        {
            if(!GeneratePath(unit))
            {
                unit.ClearJobs();
            }
        }
    }      

    private bool GeneratePath(IUnit unit)
    {
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < 5; i++)
        {
            var nextDest = GetNextSquarePosition(GetPosition(), i);
            if (Vector3.Distance(nextDest, unit.transform.position) < 0.1f)
            {
                unit.JobFinished();
                return true;
            }
            unit.m_navAgent.CalculatePath(nextDest, path);

            // Check if square is reachable
            if (path.status.Equals(NavMeshPathStatus.PathPartial) || path.status.Equals(NavMeshPathStatus.PathInvalid))
            {
                if (_OnlyCenter)
                {
                    Debug.Log("Center is not reachable");
                    return false;
                }

                if (nextDest.Equals(new Vector3(-1, 0, 0)))
                {
                    return false;
                }
            }
            else
            {
                _FinalDestination = nextDest;
                unit.m_navAgent.SetPath(path);
                return true;
            }
        }
        return false;
    }

    private Vector3 GetNextSquarePosition(Vector3 baseDest, int iteration)
    {
        switch (iteration)
        {
            case 0:
                return new Vector3(baseDest.x, baseDest.y, baseDest.z);
            case 1:
                return new Vector3(baseDest.x - 0.75f, baseDest.y, baseDest.z);
            case 2:
                return new Vector3(baseDest.x, baseDest.y, baseDest.z + 0.75f);
            case 3:
                return new Vector3(baseDest.x + 0.75f, baseDest.y, baseDest.z);
            case 4:
                return new Vector3(baseDest.x, baseDest.y, baseDest.z - 0.75f);
            case 5:
                return new Vector3(-1, 0, 0);
            default:
                break;
        }
        return new Vector3(-1, 0, 0);
    }

    private bool UnitHasReachedDestination(IUnit unit)
    {
        if (Vector3.Distance(_FinalDestination, unit.transform.position) < 0.1f)
        {
            unit.m_navAgent.ResetPath();
            return true;
        }
        return false;
    }

}

