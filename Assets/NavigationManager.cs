using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationManager : MonoBehaviour {

    NavMeshSurface surface;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BuildMesh()
    {
        surface.BuildNavMesh();
        
    }

    public void REBuildMesh()
    {
        surface.UpdateNavMesh(surface.navMeshData);
    }
}
