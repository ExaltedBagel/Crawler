using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildToolManager : MonoBehaviour {

    public MapGenerator map;
    public GameObject indicator;
    public Tile[,] currentFloor;
    public int activeLevel;
    IBuildTool currentTool;
    public Vector3 startPosition;
    public Vector3 lastPosition;
    Animator anim;
    public JobManager jobManager;

    //DEBUG
    public GameObject TA;

    void Awake()
    {
        currentTool = new DigTool(this);
        lastPosition = new Vector3();
        startPosition = new Vector3();
        activeLevel = 0;
        anim = GetComponentInChildren<Animator>();
    }
	// Use this for initialization
	void Start () {
        currentFloor = map.GetComponent<MapGenerator>().Floors[0];
        indicator = Instantiate(indicator);
    }

    // Update is called once per frame
    void Update ()
    {
        currentTool.MouseMoveFunction(ref lastPosition);
        //UpdateIndicatorPosition();

        if (Input.GetMouseButtonDown(0))
            currentTool.LeftClickFunction();

        if (Input.GetMouseButtonUp(0))
            currentTool.LeftReleaseFunction();

        if (Input.GetMouseButtonDown(1))
            currentTool.RightClickFunction();

        if (Input.GetMouseButtonUp(1))
            currentTool.RightReleaseFunction();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnitManager.SpawnGoblin();
        }
        
    }

    public Tile GetTileAtLocation(Vector3 position)
    {
        //Sanity Check
        if (position.x < 1 || position.z < 1 || Mathf.FloorToInt(position.x) + 1 >= currentFloor.GetLength(0) || Mathf.FloorToInt(position.z) + 1 >= currentFloor.GetLength(1))
            return null;
        Tile tile = null;
      
        tile = currentFloor[Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z)];

        return tile;
    }

    public ChunkDrawer GetChunkAtLocation(Vector3 position)
    {
        if (position.x < 0 || position.z < 0 || Mathf.FloorToInt(position.x) >= currentFloor.GetLength(0) || Mathf.FloorToInt(position.z) >= currentFloor.GetLength(1))
            return null;
        int chunkX = Mathf.FloorToInt(position.x / map.ChunkSize);
        int chunkZ = Mathf.FloorToInt(position.z / map.ChunkSize);

        ChunkDrawer chunk = GameObject.Find("chunk_" + activeLevel + "_" + chunkX + "_" + chunkZ).GetComponent<ChunkDrawer>();

        return chunk;
    }

    public void SetStartDragLocation()
    {
        startPosition = indicator.transform.position;
    }

    public void UpdateIndicatorSize()
    {
        //Set the scale
        Vector3 diff = new Vector3(Mathf.FloorToInt(lastPosition.x - startPosition.x), 0, Mathf.FloorToInt(lastPosition.z - startPosition.z));
        Vector3 newScale = diff;
        if (diff.x < 0)
            newScale += new Vector3(-1.25f, 1.25f, 0);
        else
            newScale += new Vector3(1.25f, 1.25f, 0);

        if (diff.z < 0)
            newScale += new Vector3(0, 0, -1.25f);
        else
            newScale += new Vector3(0, 0, 1.25f);

        indicator.transform.localScale = newScale;
        indicator.transform.position = startPosition + new Vector3((diff.x) * 0.5f, 0, (diff.z)*0.5f);
    }

    public void UpdateIndicatorPosition()
    {
        indicator.transform.position = lastPosition;
    }

    /// <summary>
    /// Returns drag and drop tile locations as floats to determine the bounding rectangle.
    /// </summary>
    /// <param name="tileTopLeft"></param>
    /// <param name="tileBottomRight"></param>
    public void GetBoxBounds(out Vector2 tileTopLeft, out Vector2 tileBottomRight)
    {
        //Get top left corner
        if(startPosition.x < lastPosition.x)
        {
            if(startPosition.z > lastPosition.z)
            {
                tileTopLeft = new Vector2(startPosition.x, startPosition.z);
            }
            else
            {
                tileTopLeft = new Vector2(startPosition.x, lastPosition.z);
            }
            
        }
        else
        {
            if (startPosition.z > lastPosition.z)
            {
                tileTopLeft = new Vector2(lastPosition.x, startPosition.z);
            }
            else
            {
                tileTopLeft = new Vector2(lastPosition.x, lastPosition.z);
            }
        }

        //Get bottom right corner
        if (startPosition.x > lastPosition.x)
        {
            if (startPosition.z < lastPosition.z)
            {
                tileBottomRight = new Vector2(startPosition.x, startPosition.z);
            }
            else
            {
                tileBottomRight = new Vector2(startPosition.x, lastPosition.z);
            }

        }
        else
        {
            if (startPosition.z < lastPosition.z)
            {
                tileBottomRight = new Vector2(lastPosition.x, startPosition.z);
            }
            else
            {
                tileBottomRight = new Vector2(lastPosition.x, lastPosition.z);
            }
        }
    }

    public void RebuildFloor(int startX, int endX, int startZ, int endZ)
    {

        for(int i = startX/map.ChunkSize; i <= endX / map.ChunkSize; i++)
        {
            for(int j = startZ / map.ChunkSize; j <= endZ / map.ChunkSize; j++)
            {
                ChunkDrawer chunk = GameObject.Find("chunk_" + activeLevel + "_" + i + "_" + j).GetComponent<ChunkDrawer>();
                chunk.BuildMesh();
            }
        }

        map.REMakeNavMesh();
    }

    public void RebuildFloor(int startX, int endX, int startZ, int endZ, int level)
    {

        for (int i = startX / map.ChunkSize; i <= endX / map.ChunkSize; i++)
        {
            for (int j = startZ / map.ChunkSize; j <= endZ / map.ChunkSize; j++)
            {
                ChunkDrawer chunk = GameObject.Find("chunk_" + level + "_" + i + "_" + j).GetComponent<ChunkDrawer>();
                chunk.BuildMesh();
            }
        }

        map.REMakeNavMesh();
    }

    public void ResetIndicatorSize()
    {
        indicator.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }

    public Orientation UpdateIndicatorOrientation()
    {
        if(lastPosition.x > startPosition.x)
        {
            //More to the right
            if(lastPosition.x - startPosition.x > Mathf.Abs(lastPosition.z - startPosition.z))
            {
                indicator.transform.localScale = new Vector3(2.50f, 1.25f, 1.25f);
                indicator.transform.position = startPosition + new Vector3(0.75f, 0, 0);
                return Orientation.RIGHT;
            }
            //More up
            else if (lastPosition.z - startPosition.z > 0)
            {
                indicator.transform.localScale = new Vector3(1.25f, 1.25f, 2.50f);
                indicator.transform.position = startPosition + new Vector3(0, 0, 0.75f);
                return Orientation.UP;
            }
            //More down
            else
            {
                indicator.transform.localScale = new Vector3(1.25f, 1.25f, -2.50f);
                indicator.transform.position = startPosition + new Vector3(0, 0, -0.75f);
                return Orientation.DOWN;            }
        }
        else
        {
            //More to the left
            if (startPosition.x - lastPosition.x > Mathf.Abs(lastPosition.z - startPosition.z))
            {
                indicator.transform.localScale = new Vector3(-2.50f, 1.25f, 1.25f);
                indicator.transform.position = startPosition + new Vector3(-0.75f, 0, 0);
                return Orientation.LEFT;
            }
            //More up
            else if (lastPosition.z - startPosition.z > 0)
            {
                indicator.transform.localScale = new Vector3(1.25f, 1.25f, 2.50f);
                indicator.transform.position = startPosition + new Vector3(0, 0, 0.75f);
                return Orientation.UP;
            }
            //More down
            else
            {
                indicator.transform.localScale = new Vector3(1.25f, 1.25f, -2.50f);
                indicator.transform.position = startPosition + new Vector3(0, 0, -0.75f);
                return Orientation.DOWN;
            }
        }
    }

    public void SelectTool(int toolNum)
    {
        if (toolNum < 0 || toolNum >= (int)Tools.TOTAL)
        {
            Debug.LogWarning("Invalid tool number. Aborting switch");
            return;
        }

        Tools tool = (Tools)toolNum;
        switch (tool)
        {
            case Tools.DIG:
                currentTool = new DigTool(this);
                break;
            case Tools.HOLE:
                currentTool = new HoleTool(this);
                break;
            case Tools.RAMP:
                currentTool = new SlopeTool(this);
                break;
            default:
                break;
        }
    }

    public bool IsTileValid(int x, int z, int floorNum, TileContent type)
    {
        Debug.Log("Floor num: " + floorNum);
        Tile[,] floor = map.Floors[floorNum];
        Debug.Log("Content under is " + floor[x, z].Content);
        return floor[x, z].Content.Equals(type);
        
    }

    public void IncrementFloor()
    {
        if(activeLevel < map.NumFloors - 1)
        {
            activeLevel++;
            map.GotoFloor(activeLevel);
            currentFloor = map.Floors[activeLevel];
        }
    }

    public void DecrementFloor()
    {
        if (activeLevel > 0)
        {
            activeLevel--;
            map.GotoFloor(activeLevel);
            currentFloor = map.Floors[activeLevel];
        }
    }

    public enum Tools
    {
        DIG,
        HOLE,
        RAMP,
        TOTAL
    }

    public enum Orientation
    {
        LEFT,
        UP,
        RIGHT,
        DOWN
    }

}
