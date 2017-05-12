using System;
using System.Collections.Generic;
using UnityEngine;

public class SlopeTool : IBuildTool
{
    BuildToolManager.Orientation lastOrientation;

    public SlopeTool(BuildToolManager manager) : base("Dig", manager)
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
                    case State.FIRST_CLICK:
                        lastOrientation = manager.UpdateIndicatorOrientation();
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
        if (state.Equals(State.STANDARD))
        {
            //Current tile must be floor, tile under must be wall
            if (activeTile != null)
            {
                int activeX = Mathf.RoundToInt(manager.indicator.transform.position.x);
                int activeZ = Mathf.RoundToInt(manager.indicator.transform.position.z);
                if (activeTile.Content.Equals(TileContent.FLOOR) && manager.IsTileValid(activeX,activeZ, manager.activeLevel + 1, TileContent.WALL))
                {
                    //OnClick, goes down a level so you can choose where to ramp to
                    manager.SetStartDragLocation();
                    state = State.FIRST_CLICK;
                    manager.IncrementFloor();
                }
                
            }
        }
        else if(state.Equals(State.FIRST_CLICK))
        {
            //Indicator scale tells us which direction is affected.
            int activeX = Mathf.FloorToInt(manager.startPosition.x);
            int activeZ = Mathf.FloorToInt(manager.startPosition.z);
            int bottomX;
            int bottomZ;
            TileContent dirDown;
            TileContent dirUp;

            switch (lastOrientation)
            {
                case BuildToolManager.Orientation.LEFT:
                    bottomX = activeX - 1;
                    bottomZ = activeZ;
                    dirDown = TileContent.SLOPE_D_L;
                    dirUp = TileContent.SLOPE_U_R;
                    break;
                case BuildToolManager.Orientation.UP:
                    bottomX = activeX;
                    bottomZ = activeZ + 1;
                    dirDown = TileContent.SLOPE_D_U;
                    dirUp = TileContent.SLOPE_U_D;
                    break;
                case BuildToolManager.Orientation.RIGHT:
                    bottomX = activeX + 1;
                    bottomZ = activeZ;
                    dirDown = TileContent.SLOPE_D_R;
                    dirUp = TileContent.SLOPE_U_L;
                    break;
                case BuildToolManager.Orientation.DOWN:
                    bottomX = activeX;
                    bottomZ = activeZ - 1;
                    dirDown = TileContent.SLOPE_D_D;
                    dirUp = TileContent.SLOPE_U_U;
                    break;
                default:
                    bottomX = activeX;
                    bottomZ = activeZ;
                    dirDown = TileContent.FLOOR;
                    dirUp = TileContent.FLOOR;
                    break;
            }

            //Slope is valid here
            if (manager.IsTileValid(bottomX, bottomZ, manager.activeLevel, TileContent.FLOOR))
            {
                /*
                manager.currentFloor[bottomX, bottomZ].Content = dirUp;
                manager.currentFloor[activeX, activeZ].Content = TileContent.UNDERSLOPE;
                manager.RebuildFloor(bottomX - 1, bottomX + 1, activeZ - 1, activeZ + 1);
                manager.DecrementFloor();
                manager.currentFloor[activeX, activeZ].Content = dirDown;
                manager.RebuildFloor(bottomX - 1, bottomX + 1, activeZ - 1, activeZ + 1);
                manager.map.GotoFloor(manager.activeLevel);
                */
                manager.DecrementFloor();

                switch (dirDown)
                {                    
                    case TileContent.SLOPE_D_L:
                        manager.jobManager.AddNewJob(new JobRamp(activeX, activeZ, manager.activeLevel, 0));
                        break;
                    case TileContent.SLOPE_D_U:
                        manager.jobManager.AddNewJob(new JobRamp(activeX, activeZ, manager.activeLevel, 1));
                        break;
                    case TileContent.SLOPE_D_R:
                        manager.jobManager.AddNewJob(new JobRamp(activeX, activeZ, manager.activeLevel, 2));
                        break;
                    case TileContent.SLOPE_D_D:
                        manager.jobManager.AddNewJob(new JobRamp(activeX, activeZ, manager.activeLevel, 3));
                        break;
                    default:
                        break;
                }
                manager.ResetIndicatorSize();
                state = State.STANDARD;
            }
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

        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                if (manager.currentFloor[i, j].Content.Equals(TileContent.WALL))
                    manager.currentFloor[i, j].Content = TileContent.FLOOR;
            }
        }

        manager.RebuildFloor(startX, endX, startZ, endZ);
        manager.ResetIndicatorSize();

    }

    public override void RightClickFunction()
    {
        if (!state.Equals(State.STANDARD))
        {
            state = State.STANDARD;
            manager.ResetIndicatorSize();
            manager.DecrementFloor();
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

