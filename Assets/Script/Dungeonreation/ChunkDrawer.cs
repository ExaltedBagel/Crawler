using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkDrawer : MonoBehaviour {

    MapGenerator map;
    MeshRenderer mr;
    MeshFilter mf;
    MeshCollider mc;
    int XIndex { get; set; }
    int YIndex { get; set; }
    int ZIndex { get; set; }

    //Get all the pointers
    void Awake()
    {
        map = GameObject.Find("Map").GetComponent<MapGenerator>();
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
    }

	public void SetupChunk(int xIndex, int zIndex, int yIndex)
    {
        XIndex = xIndex;
        YIndex = yIndex;
        ZIndex = zIndex;
    }

    // Builds the mesh for the floor
    public void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        //Select the correct floor
        Tile[,] floor = map.Floors[YIndex];

        List<Vector3> verts = new List<Vector3>();
        verts.Capacity = 100;

        List<Vector3> norms = new List<Vector3>();
        norms.Capacity = 100;

        List<Vector2> uvs = new List<Vector2>();
        uvs.Capacity = 100;

        //Make a list of the tris
        List<List<int>> triList = new List<List<int>>();
        for (int i = 0; i < 4; i++)
        {
            List<int> tris = new List<int>();
            tris.Capacity = 400;
            triList.Add(tris);
        }

        int currentVert = 0;

        int startX = XIndex * map.ChunkSize;
        int endX = (XIndex + 1) * map.ChunkSize;
        int startZ = ZIndex * map.ChunkSize;
        int endZ = (ZIndex + 1) * map.ChunkSize;

        List<Rectangle> batch = BatchTiles(floor, TileContent.FLOOR, startX, endX, startZ, endZ);

        DrawFloorBatch(verts, norms, triList[0], uvs, batch, ref currentVert);

        List<int> tris2 = new List<int>();
        batch = BatchTiles(floor, TileContent.WALL, startX, endX, startZ, endZ);
        DrawWallBatch(verts, norms, triList[1], uvs, batch, ref currentVert);                
        
        batch = BatchTiles(floor, TileContent.SLOPE_D_L, startX, endX, startZ, endZ);
        DrawRampDLBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_D_R, startX, endX, startZ, endZ);
        DrawRampDRBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_D_D, startX, endX, startZ, endZ);
        DrawRampDDBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_D_U, startX, endX, startZ, endZ);
        DrawRampDUBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_U_R, startX, endX, startZ, endZ);
        DrawRampURBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_U_L, startX, endX, startZ, endZ);
        DrawRampULBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_U_D, startX, endX, startZ, endZ);
        DrawRampUDBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.SLOPE_U_U, startX, endX, startZ, endZ);
        DrawRampUUBatch(verts, norms, triList[0], triList[1], uvs, batch, ref currentVert);

        batch = BatchTiles(floor, TileContent.UNDERSLOPE, startX, endX, startZ, endZ);
        DrawLowWallBatch(verts, norms, triList[1], uvs, batch, ref currentVert);


        /*  
        batch = BatchTiles(floor, TileContent.HOLE, startX, endX, startZ, endZ);
        var edges = GenerateEdged(batch);
        DrawHoleBatch(verts, norms, tris, edges, ref currentVert);

        */
        //Assign stuff to the thing
        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.SetTriangles(triList[0], 0);
        mesh.SetTriangles(triList[1], 1);
        mesh.SetUVs(0, uvs);

        //Sanity checks
        if (mesh.GetTriangles(1).Length < 3)
        {
            mesh.SetTriangles(new int[3] { 0, 0, 0 }, 1);
        }


        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh.Clear();
        mf.mesh = mesh;
        mc.sharedMesh = mf.mesh;

        //GetComponent<NavigationManager>().BuildMesh();
        
    }    

    //BatchFloors and draw in optimized way
    public List<Rectangle> BatchTiles(Tile[,] floor, TileContent toBatch,int startX, int endX, int startZ, int endZ)
    {        
        bool[,] canBlob = new bool[endX - startX + 1, endZ - startZ + 1];

        for(int i = startX; i < endX; i++)
        {
            for(int j = startZ; j < endZ; j++)
            {
                if(floor[i, j].Content.Equals(toBatch))
                {
                    canBlob[i - startX, j - startZ] = true;
                }
                else
                {
                    canBlob[i - startX, j - startZ] = false;
                }
            }
        }

        //Make optimal rectangles
        List<Rectangle> rect = new List<Rectangle>();

        //BEGIN THE BLOB!
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j < endZ; j++)
            {
                if(canBlob[i - startX, j - startZ])
                {
                    //Blob initial space
                    canBlob[i - startX, j - startZ] = false;

                    //Blob to the right
                    int finalZ = j;
                    while(finalZ < endZ - 1)
                    {
                        if (canBlob[i - startX, finalZ - startZ + 1])
                        {
                            finalZ++;
                            canBlob[i - startX, finalZ - startZ] = false;
                        }
                        else
                            break;
                    }

                    //Blob up
                    int finalX = i;
                    bool canBlobUp = true;

                    while(finalX < floor.GetLength(0) - 1 && canBlobUp)
                    { 
                        //Check if you can blob the whole row on top
                        for(int l = j; l <= finalZ && canBlobUp; l++)
                        {
                            if (!canBlob[finalX + 1 - startX, l - startZ])
                            {
                                canBlobUp = false;
                            }                            
                        }

                        //Blob all the row at once
                        if(canBlobUp)
                        {
                            finalX++;
                            for (int l = j; l <= finalZ; l++)
                            {
                                canBlob[finalX - startX, l - startZ] = false;
                            }
                        }
                    }

                    //Make the rectangle with the final blob
                    rect.Add(new Rectangle(finalZ, i, j, finalX));
                }
            }
        }

        //Keep it if it is clean
        rect = rect.OrderByDescending(e => e.Area()).ToList();

        return rect;
    }

    public static List<Edge> GenerateEdged(List<Rectangle> rects)
    {
        List<Edge> edges = new List<Edge>();

        foreach (Rectangle r in rects)
        {
            edges.AddRange(Edge.ExtractEdges(r));            
        }

        //Cleanup
        List<Edge> finalEdges = new List<Edge>();
        int i = 0;
        int j = 1;
        while(i < edges.Count - 1)
        {
            if(Edge.EdgesOverlap(edges[i], edges[j]))
            {
                edges.AddRange(Edge.SubtractEdges(edges[i], edges[j]));
                edges.RemoveAt(i);
                edges.RemoveAt(j - 1);
                j = i + 1;
            }
            else
            {
                j++;
            }
            if(j >= edges.Count )
            {
                i++;
                j = i + 1;
            }
        }

        return edges;
    }

    void DrawFloorBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Top of slope
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris.Add(currentVert);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX =  ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, uvY));
            uvs.Add(new Vector2(uvX, uvY));
            uvs.Add(new Vector2(uvX, 0.0f));


            currentVert += 4;
            
        }
    }

    void DrawWallBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Draw top
            verts.Add(new Vector3(e.left() - 0.5f   , 1.00f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f   , 1.00f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f  , 1.00f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f  , 1.00f, e.bottom() - 0.5f));

            tris.Add(currentVert);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, uvY));
            uvs.Add(new Vector2(uvX, uvY));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;

            //Draw contour
            //LEFT
            verts.Add(new Vector3(e.left() - 0.5f, 1.00f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 1.00f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(uvY, 1.0f));

            currentVert += 4;

            //RIGHT
            verts.Add(new Vector3(e.right() + 0.5f, 1.00f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 1.00f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvY, 1.0f));
            uvs.Add(new Vector2(uvY, 0.0f));

            currentVert += 4;

            //TOP
            verts.Add(new Vector3(e.right() + 0.5f, 1.00f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 1.00f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;

            //BOTTOM
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 1.00f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 1.00f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;
        }
    }

    void DrawHoleBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<Edge> patches, ref int currentVert)
    {
        foreach (Edge e in patches)
        {
            //Draw two sides of the edge
            verts.Add(new Vector3(e.start.x - 0.5f, -0.2f, e.start.z + 0.5f));
            verts.Add(new Vector3(e.start.x - 0.5f, 0.0f, e.start.z + 0.5f));
            verts.Add(new Vector3(e.end.x - 0.5f, 0.0f, e.end.z + 0.5f));
            verts.Add(new Vector3(e.end.x - 0.5f, -0.2f, e.end.z + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;

            verts.Add(new Vector3(e.start.x - 0.5f, -0.2f, e.start.z + 0.5f));
            verts.Add(new Vector3(e.start.x - 0.5f, 0.0f, e.start.z + 0.5f));
            verts.Add(new Vector3(e.end.x - 0.5f, 0.0f, e.end.z + 0.5f));
            verts.Add(new Vector3(e.end.x - 0.5f, -0.2f, e.end.z + 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;
        }

        /*
        foreach (Edge e in patches)
        {
            //Draw contour
            //LEFT
            verts.Add(new Vector3(e.left() - 0.5f, -0.2f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.2f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;

            //RIGHT
            verts.Add(new Vector3(e.right() + 0.5f, -0.2f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.2f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;

            //TOP
            verts.Add(new Vector3(e.right() + 0.5f, -0.2f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.2f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;

            //BOTTOM
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.2f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.2f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            currentVert += 4;
        }
        */
    }

    void DrawRampDLBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris.Add(currentVert);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));

            //TODO: UVs
            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvY, 1.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampDRBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvY, 1.0f));
            uvs.Add(new Vector2(uvY, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));

            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));            

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvY, 1.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampDDBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(uvX, 0.5f));

            //TODO: UVs
            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampDUBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(uvX, 0.5f));

            //TODO: UVs
            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 1.0f));

            //TODO: UVs
            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, -0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, -0.6f, e.bottom() - 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 1.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampURBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));

            //TODO: UVs
            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 0.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 0.0f));

            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampULBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvY, 0.5f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            currentVert += 3;

            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampUDBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));

            //TODO: UVs
            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            //TODO: UVs
            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.bottom() - 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));

            currentVert += 4;
        }
    }

    void DrawRampUUBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<int> tris2, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Slope part
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;

            //Sides of slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 1);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(1.0f, 0.0f));

            currentVert += 3;

            verts.Add(new Vector3(e.left() - 0.5f, 0.60f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0, e.top() + 0.5f));

            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 2);

            for (int k = 0; k < 3; k++)
                norms.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(1.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(1.0f, 0.0f));

            currentVert += 3;

            //Back of the slope
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));

            tris2.Add(currentVert + 1);
            tris2.Add(currentVert + 0);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 3);
            tris2.Add(currentVert + 2);
            tris2.Add(currentVert + 0);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));

            currentVert += 4;
        }
    }

    void DrawLowWallBatch(List<Vector3> verts, List<Vector3> norms, List<int> tris, List<Vector2> uvs, List<Rectangle> patches, ref int currentVert)
    {
        foreach (Rectangle e in patches)
        {
            //Draw top
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert);
            tris.Add(currentVert + 1);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 3);
            tris.Add(currentVert);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            float uvX = ((e.right() + 1) - e.left());
            float uvY = ((e.top() + 1) - e.bottom());

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, uvY));
            uvs.Add(new Vector2(uvX, uvY));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;

            //Draw contour
            //LEFT
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;

            //RIGHT
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvY, 0.0f));
            uvs.Add(new Vector2(uvY, 0.5f));

            currentVert += 4;

            //TOP
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.top() + 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.top() + 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.top() + 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(uvX, 0.0f));
            uvs.Add(new Vector2(uvX, 0.5f));

            currentVert += 4;

            //BOTTOM
            verts.Add(new Vector3(e.right() + 0.5f, 0.0f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.right() + 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.6f, e.bottom() - 0.5f));
            verts.Add(new Vector3(e.left() - 0.5f, 0.0f, e.bottom() - 0.5f));

            tris.Add(currentVert + 1);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 2);
            tris.Add(currentVert + 0);
            tris.Add(currentVert + 3);

            for (int k = 0; k < 4; k++)
                norms.Add(new Vector3(0, 1, 0));

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.5f));
            uvs.Add(new Vector2(uvX, 0.5f));
            uvs.Add(new Vector2(uvX, 0.0f));

            currentVert += 4;
        }
    }

}
