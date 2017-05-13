using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour {

    public JobManager mJobManager;

    static public List<IUnit> population { get; set; }

    void Awake()
    {
        population = new List<IUnit>();
    }

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Population going to sleep");
            foreach (IUnit x in population)
            {
                x.GoToSleep();
            }
        }
    }

    public IUnit GetFreeUnit()
    {
        lock(population)
        {
            foreach (IUnit unit in population)
            {
                if (!unit.HasAJob())
                    return unit;
            }
            return null;
        }        
    }

    public static void SpawnGoblin()
    {
        GameObject gobRes = Resources.Load("Prefabs/goblin", typeof(GameObject)) as GameObject;
        var newGob = Instantiate(gobRes, MapGenerator.entrance, Quaternion.identity);
        population.Add(newGob.GetComponent<IUnit>());
        newGob.GetComponent<Goblin>().ClearJobs();
    }

    /*
    void FindJobsForIdles()
    {
        foreach (IUnit x in population)
        {
            if(!x.HasAJob())
            {
                JobManager.FindJobForUnit(x);
            }
        }
    }
    */
}
