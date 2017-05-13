using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour {

    public UnitManager unitManager;
    public static JobManager jobManager;
    static List<IJob> vacantJobs;
    static List<IJob> occupiedJobs;
    static List<IJob> innaccessibleJobs;
    Object mLock;

    void Awake()
    {
        vacantJobs = new List<IJob>();
        occupiedJobs = new List<IJob>();
        innaccessibleJobs = new List<IJob>();
        jobManager = GameObject.Find("GameManager").GetComponent<JobManager>();
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(UpdateJobAccessibility());
        StartCoroutine(UpdateJobAdjacency());
    }

    // Update is called once per frame
    void Update () {
		
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

                //IUnit freeUnit = unitManager.GetFreeUnit();
                //
                //if (freeUnit == null)
                //{
                //    vacantJobs.Add(job);
                //}
                //else
                //{
                //    job.AssignUnit(freeUnit);
                //    occupiedJobs.Add(job);
                //}
                if(Tile.IsTileAccessible(job.GetPosition()))
                {
                    vacantJobs.Add(job);
                }
                else
                {
                    innaccessibleJobs.Add(job);
                }
                UpdateAdjacentJobs(job);
            } 
        }
    }

    public static void UpdateJobProgress(IUnit unit)
    {

    }

    private void UpdateAdjacentJobs(IJob job)
    {
        // Add all adjacent jobs
        foreach (IJob x in vacantJobs)
        {
            if (x.Equals(job) || x.state.Equals(IJob.State.DONE))
                continue;
            if (Vector3.Distance(x.GetPosition(), job.GetPosition()) < 1.5f && x.Level == job.Level)
                job.AdjacentJobs.Add(x);
        }

        foreach (IJob x in innaccessibleJobs)
        {
            if (x.Equals(job) || x.state.Equals(IJob.State.DONE))
                continue;
            if (Vector3.Distance(x.GetPosition(), job.GetPosition()) < 1.5f && x.Level == job.Level)
                job.AdjacentJobs.Add(x);
        }

        Debug.Log("Job has " + job.AdjacentJobs.Count + " adjacent jobs");

    }

    private IEnumerator UpdateJobAccessibility()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            lock (innaccessibleJobs)
            {
                for (int i = innaccessibleJobs.Count - 1; i >= 0; i--)
                {
                    if (Tile.IsTileAccessible(innaccessibleJobs[i].GetPosition()))
                    {
                        vacantJobs.Add(innaccessibleJobs[i]);
                        innaccessibleJobs.RemoveAt(i);
                    }
                }
            }
        }        
    }

    private IEnumerator UpdateJobAdjacency()
    {
        while(true)
        {
            yield return new WaitForSeconds(2.0f);
            foreach (IJob x in vacantJobs)
            {
                UpdateAdjacentJobs(x);
            }
            yield return new WaitForFixedUpdate();
            foreach (IJob x in occupiedJobs)
            {
                UpdateAdjacentJobs(x);
            }
            yield return new WaitForFixedUpdate();
            foreach (IJob x in innaccessibleJobs)
            {
                UpdateAdjacentJobs(x);
            }
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

    public static void JobWasCleared(IJob job, bool wasInnaccessible = false)
    {
        if (job == null)
            return;
        lock (vacantJobs)
        {
            lock (occupiedJobs)
            {
                if(job.IsDone())
                {
                    occupiedJobs.Remove(job);
                }
                else
                {
                    if (wasInnaccessible)
                        innaccessibleJobs.Add(job);
                    else
                        vacantJobs.Add(job);
                    occupiedJobs.Remove(job);
                }
            }
        }
    }
}
