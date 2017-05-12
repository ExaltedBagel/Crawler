using System;
using System.Collections.Generic;
using UnityEngine;

public class DigTool : IBuildTool
{
    public DigTool(BuildToolManager manager) : base("Dig", manager)
    {
        
    }

    /// <summary>
    /// Update the marker position
    /// </summary>
    public override void MouseMoveFunction(ref Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;

        //Try raycast on top plan and see if a wall is under
        if (topPlane.Raycast(ray, out rayDistance))
        {
            Vector3 newPosition = ray.GetPoint(rayDistance);
            Tile tile = manager.GetTileAtLocation(newPosition);
            if (tile != null)
            {
                activeTile = tile;
                //activeChunk = manager.GetChunkAtLocation(newPosition);
                position = new Vector3(Mathf.FloorToInt(newPosition.x), 0.5f, Mathf.FloorToInt(newPosition.z));

                switch (state)
                {
                    case State.STANDARD:
                        manager.UpdateIndicatorPosition();
                        break;
                    case State.DRAG_LEFT:
                        manager.UpdateIndicatorSize();
                        break;
                    case State.DRAG_RIGHT:
                        manager.UpdateIndicatorSize();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override void LeftClickFunction()
    {
        if (!state.Equals(State.STANDARD))
        {
            state = State.STANDARD;
            manager.ResetIndicatorSize();
            return;
        }

        if (activeTile != null)
        {
            manager.SetStartDragLocation();
            state = State.DRAG_LEFT;
        }
    }

    public override void LeftReleaseFunction()
    {
        if (!state.Equals(State.DRAG_LEFT))
            return;

        state = State.STANDARD;

        //Mark all walls selected for digging!
        Vector2 topLeft;
        Vector2 bottomRight;
        manager.GetBoxBounds(out topLeft, out bottomRight);

        int startX = Mathf.FloorToInt(topLeft.x);
        int endX = Mathf.FloorToInt(bottomRight.x);

        int startZ = Mathf.FloorToInt(bottomRight.y);
        int endZ = Mathf.FloorToInt(topLeft.y);

        for(int i = startX; i <= endX; i++)
        {
            for(int j = startZ; j <= endZ; j++)
            {
                if (manager.currentFloor[i, j].Content.Equals(TileContent.WALL))
                {
                    manager.jobManager.AddNewJob(new JobDig(i, j, manager.activeLevel));
                    //manager.currentFloor[i, j].Content = TileContent.FLOOR;
                }
            }
        }

        //manager.RebuildFloor(startX, endX, startZ, endZ);
        manager.ResetIndicatorSize();

    }

    public override void RightClickFunction()
    {
        if (!state.Equals(State.STANDARD))
        {
            state = State.STANDARD;
            manager.ResetIndicatorSize();
            return;
        }

        if (activeTile != null)
        {
            manager.SetStartDragLocation();
            state = State.DRAG_RIGHT;
        }
    }

    public override void RightReleaseFunction()
    {
        if (!state.Equals(State.DRAG_RIGHT))
            return;

        state = State.STANDARD;

        //Mark all walls selected for digging!
        Vector2 topLeft;
        Vector2 bottomRight;
        manager.GetBoxBounds(out topLeft, out bottomRight);

        int startX = Mathf.FloorToInt(topLeft.x);
        int endX = Mathf.FloorToInt(bottomRight.x);

        int startZ = Mathf.FloorToInt(bottomRight.y);
        int endZ = Mathf.FloorToInt(topLeft.y);

        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                if (manager.currentFloor[i, j].Content.Equals(TileContent.FLOOR))
                    manager.currentFloor[i, j].Content = TileContent.WALL;
            }
        }

        manager.RebuildFloor(startX, endX, startZ, endZ);
        manager.ResetIndicatorSize();
    }
}

