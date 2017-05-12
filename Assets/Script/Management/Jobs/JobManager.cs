using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour {

    public UnitManager unitManager;
    public static JobManager jobManager;
    static List<IJob> vacantJobs;
    static List<IJob> occupiedJobs;
    Object mLock;

    void Awake()
    {
        vacantJobs = new List<IJob>();
        occupiedJobs = new List<IJob>();
        jobManager = GameObject.Find("GameManager").GetComponent<JobManager>();
    }

	// Use this for initialization
	void Start () {
        //InvokeRepeating("AssignUnitsToJobs", 5.0f, 5.0f);
    }

    // Update is called once per frame
    void Update () {
		
	}

    //Check every now and then if a job exists and if someone can take it.
    void AssignUnitsToJobs()
    {
        for(int i = vacantJobs.Count - 1; i >= 0; i--)
        {
            IUnit freeUnit = unitManager.GetFreeUnit();
            if (freeUnit == null)
                break;
            else
            {
                vacantJobs[i].AssignUnit(freeUnit);
                occupiedJobs.Add(vacantJobs[i]);
                vacantJobs.RemoveAt(i);
            }
        }
    }

    public void AddNewJob(IJob job)
    {
        //Check if job exists
        lock (vacantJobs)
        {
            lock (occupiedJobs)
            {
                foreach (IJob x in vacantJobs)
                {
                    if (x.Equals(job))
                        return;
                }

                foreach (IJob x in occupiedJobs)
                {
                    if (x.Equals(job))
                        return;
                }

                IUnit freeUnit = unitManager.GetFreeUnit();

                if (freeUnit == null)
                {
                    vacantJobs.Add(job);
                }
                else
                {
                    job.AssignUnit(freeUnit);
                    occupiedJobs.Add(job);
                }
            } 
        }
    }

    public static void UpdateJobProgress(IUnit unit)
    {
        if (unit.m_currentJob.progress <= 0)
            unit.m_currentJob.state = IJob.State.DONE;

        if(unit.m_currentJob.state.Equals(IJob.State.DONE))
        {
            lock (vacantJobs)
            {
                lock (occupiedJobs)
                {
                    occupiedJobs.Remove(unit.m_currentJob);
                    unit.m_currentJob.OnFinished();
                    unit.ClearJob();
                }
            }
        }
        else if(unit.m_currentJob.state.Equals(IJob.State.UNREACHABLE))
        {
            lock (vacantJobs)
            {
                lock (occupiedJobs)
                {
                    occupiedJobs.Remove(unit.m_currentJob);
                    vacantJobs.Add(unit.m_currentJob);
                    unit.ClearJob();
                }
            }
        }
        else if (unit.m_currentJob.state.Equals(IJob.State.PROGRESS))
        {
            unit.m_currentJob.progress--;
        }
    }

    public static bool FindJobForUnit(IUnit unit)
    {
        lock (vacantJobs)
        {
            lock (occupiedJobs)
            {
                if (vacantJobs.Count > 0)
                {                
                    vacantJobs[0].AssignUnit(unit);
                    occupiedJobs.Add(vacantJobs[0]);
                    vacantJobs.RemoveAt(0);
                    return true;                    
                }
            }
        }

        return false;
    }

    public static void JobWasCleared(IJob job)
    {
        if (job == null)
            return;
        lock (vacantJobs)
        {
            lock (occupiedJobs)
            {
                if(job.state.Equals(IJob.State.DONE))
                {
                    occupiedJobs.Remove(job);
                }
                else
                {
                    vacantJobs.Add(job);
                    occupiedJobs.Remove(job);
                }
            }

        }

    }
}
