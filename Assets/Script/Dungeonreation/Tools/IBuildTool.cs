using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class IBuildTool
{
    public State state { get; set; }
    protected Plane groundPlane;
    protected Plane topPlane;
    protected Tile activeTile;
    protected int activeX;
    protected int activeZ;
    protected ChunkDrawer activeChunk;
    protected BuildToolManager manager;

    public IBuildTool(string name, BuildToolManager manager)
    {
        this.state = State.STANDARD;
        this.manager = manager;
        this.name = name;
        activeTile = null;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        topPlane = new Plane(Vector3.up, Vector3.up);
    }

    public abstract void MouseMoveFunction(ref Vector3 position);
    public abstract void LeftClickFunction();
    public abstract void LeftReleaseFunction();
    public abstract void RightClickFunction();
    public abstract void RightReleaseFunction();

    public void MoveAgent(NavMeshAgent ag)
    {

    }

    public string name { get; }

    public enum State
    {
        STANDARD,
        DRAG_LEFT,
        DRAG_RIGHT,
        FIRST_CLICK,
        SECOND_CLICK,
        THIRD_CLICK
    }

}

