using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAgent : MonoBehaviour {

    NavMeshAgent agent;
    public IUnit unit;
    //State state;
    bool isOnNavMesh = false;
    Vector3 lastDestination = new Vector3();
    Animator anim;

   

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        unit = new Goblin();
        unit.m_navAgent = agent;
    }

	// Use this for initialization
	void Start () {
        InvokeRepeating("CheckJobStatus", 1.0f, 1.0f);

    }

    // Update is called once per frame
    void Update () {
        if(!agent.hasPath)
        {
            anim.SetInteger("moving", 0);
        }
        else
        {
            anim.SetInteger("moving", 1);
        }
	}

    public void UnitHasAJob()
    {
        InvokeRepeating("CheckJobStatus", 0.0f, 1.0f);
    }

    public void UnitHasNoJob()
    {
        CancelInvoke("CheckJobStatus");
    }

}
