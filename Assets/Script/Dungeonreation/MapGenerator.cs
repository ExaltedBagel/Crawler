using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour {

    List<Tile[,]> floors;
    public List<Tile[,]> Floors { get { return floors; } }
    public GameObject mapContainer { get; set; }

    //
    public int NumFloors;
    public int ChunkSize;
    public int ChunkX;
    public int ChunkZ;

    public static Vector3 entrance;

    static public MapGenerator MapInstance;
    
    // Use this for initialization
    void Start () {
        entrance = new Vector3(1, 0.1f, 31);
        MapInstance = this;
        //ACTUAL MAP GENERATION
        floors = new List<Tile[,]>();
        //Generate raw floor data
        GenerateMapData();
        //Generate the chunks
        GenerateChunks();
        //
        MakeNavMesh();
        //Goto floor 0
        GotoFloor(0);

    }


    //Generates the map data
    void GenerateMapData()
    {
        //Generate the floors
        for(int k = 0; k < NumFloors; k++)
        {
            floors.Add(new Tile[ChunkSize * ChunkX, ChunkSize * ChunkZ]);
            //Generate the map data itself
            Tile[,] floor = floors[k];
            for (int i = 0; i < ChunkSize * ChunkX; i++)
            {
                for (int j = 0; j < ChunkSize * ChunkZ; j++)
                {
                    floor[i, j] = new Tile(i,j);

                    if (i == 0 || j == 0 || i == ChunkSize * ChunkX - 1 || j == ChunkSize * ChunkZ - 1 )
                        floor[i, j].Content = TileContent.WALL;
                    else
                        floor[i, j].Content = TileContent.WALL;
                }
            }

        }

        //DEBUG
        Debug.LogWarning("Curently setting default values to the floor here");
        
        Tile[,] floor0 = floors[0];

        for (int i = 0; i < 3; i++)
        {

            floor0[2 + i, 30].Content = TileContent.FLOOR;
            floor0[2 + i, 31].Content = TileContent.FLOOR;
            floor0[2 + i, 32].Content = TileContent.FLOOR;

        }
        floor0[1, 31].Content = TileContent.FLOOR;

        //Add a bed to the map, and add a room in the room in question.
        GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        bed.name = "Bed";
        bed.GetComponent<BoxCollider>().enabled = false;
        var bedComp = bed.AddComponent<Bed>();
        bedComp.SetInTile(floor0[3, 30]);

        //Make a room from the bed
        Quarter quart = new Quarter(new Rectangle(32, 2, 30, 4));
        

    }


    void GenerateChunks()
    {
        mapContainer = new GameObject("MapContainer");
        var sur = mapContainer.AddComponent<NavMeshSurface>();
        sur.buildHeightMesh = true;
        sur.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        GameObject chunk = Resources.Load("Prefabs/MapChunk", typeof(GameObject)) as GameObject;
        GameObject newChunk;

        for (int k=0; k < NumFloors; k++)
        {
            GameObject Level = new GameObject("Level" + k);
            Level.transform.SetParent(mapContainer.transform);
            Level.transform.position = new Vector3(0, 1.2f * -k, 0);

            for (int i = 0; i < ChunkX; i++)
            {
                for(int j = 0; j < ChunkZ; j++)
                {                    
                    newChunk = Instantiate(chunk);
                    newChunk.name = "chunk_" + k +"_"+ i +"_"+ j;
                    newChunk.transform.SetParent(Level.transform);
                    newChunk.transform.localPosition = new Vector3(0,0,0);
                    newChunk.layer = 8 + k;

                    var data = newChunk.GetComponent<ChunkDrawer>();
                    data.SetupChunk(i, j, k);
                    data.BuildMesh();
                }
            }
           
            Level.SetActive(true);
        }
    }

    public void GotoFloor(int floor)
    {
        var cam = Camera.main;
        cam.cullingMask = 127;
        for (int i = 0; i < NumFloors; i++)
        {
            if (i < floor + 3 && i >= floor)
            {
                //var floorToActivate = mapContainer.transform.FindChild("Level" + i).gameObject.GetComponentsInChildren<MeshRenderer>();
                //
                //foreach(MeshRenderer mr in floorToActivate)
                //    mr.enabled = (true);
                
                cam.cullingMask |= 1 << (i + 8);

            }
            else
            {
                //var floorToDeactivate = mapContainer.transform.FindChild("Level" + i).gameObject.GetComponentsInChildren<MeshRenderer>();
                //foreach (MeshRenderer mr in floorToDeactivate)
                //    mr.enabled = (false);
                //var cam = Camera.main;
                //int floorToAdd = 1 << (i + 8);
                cam.cullingMask &= ~(1 << (i + 8));

            }
        }

        //mapContainer.transform.position = new Vector3(0, 1.2f * floor, 0);        
    }

    public void MakeNavMesh()
    {
        mapContainer.GetComponent<NavMeshSurface>().BuildNavMesh();
        //lv.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public void REMakeNavMesh()
    {
        var sur = mapContainer.GetComponent<NavMeshSurface>();
        lock(sur)
        {
            sur.UpdateNavMesh(sur.navMeshData);
        }
        //lv.GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
