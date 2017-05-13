using System;
using System.Collections.Generic;
using UnityEngine;


public class Tile
{
    List<Link> mLinks;

    public MarkedFor Marked { get; set; }
    public TileContent Content { get; set; }
    public IObject ObjectContained { get; set; }
    public Coordinate Coord { get; }
    int level { get; set; }

    public Tile()
    {
        mLinks = new List<Link>();
        Content = TileContent.WALL;
        Marked = MarkedFor.NOTHING;        
    }

    public Tile(int x, int z, int level)
    {
        mLinks = new List<Link>();
        Content = TileContent.WALL;
        Marked = MarkedFor.NOTHING;
        Coord = new Coordinate(x, z);
        this.level = level;
    }

    public enum MarkedFor
    {
        NOTHING,
        DIG,
        SLOPE
    }

    //Links to another tile
    public void CreateLink(Tile otherTile)
    {
        //Makes sure we are not linking a wall
        if (!otherTile.Content.Equals(TileContent.WALL))
        {
            Link l = new Link();
            l.t1 = this;
            l.t2 = otherTile;

            //Cost calculation will go here
            l.cost = 1;

            mLinks.Add(l);
        }
    }

    public static Vector3 PositionToVector3(int x, int z, int level)
    {
        return new Vector3(x, level * -1.25f, z);
    }

    public static Tile TileAtPosition(Vector3 position)
    {
        int level = Mathf.RoundToInt(-position.y / 1.25f);
        int x = Mathf.RoundToInt(position.x);
        int z = Mathf.RoundToInt(position.z);
        return MapGenerator.MapInstance.Floors[level][x,z];
    }

    public static Tile TileAtPosition(int x, int z, int level)
    {        
        return MapGenerator.MapInstance.Floors[level][x, z];
    }

    public static bool IsTileWalkable(Tile tile)
    {
        return !tile.Content.Equals(TileContent.WALL);
    }

    public static bool IsTileAccessible(int x, int z, int level)
    {
        return IsTileAccessible(TileAtPosition(x, z, level));
    }

    public static bool IsTileAccessible(Vector3 position)
    {
        return IsTileAccessible(TileAtPosition(position));
    }

    public static bool IsTileAccessible(Tile tile)
    {
        var floor = MapGenerator.MapInstance.Floors[tile.level];
        bool isAccessible = false;
        if(tile.Coord.x > 0)
            isAccessible |= IsTileWalkable(floor[tile.Coord.x - 1, tile.Coord.z]);
        if(tile.Coord.x < floor.GetLength(0) - 1)
            isAccessible |= IsTileWalkable(floor[tile.Coord.x - 1, tile.Coord.z]);
        if (tile.Coord.z > 0)
            isAccessible |= IsTileWalkable(floor[tile.Coord.x, tile.Coord.z - 1]);
        if (tile.Coord.z < floor.GetLength(1) - 1)
            isAccessible |= IsTileWalkable(floor[tile.Coord.x, tile.Coord.z + 1]);
        return isAccessible;
    }
}

struct Link
{
    public Tile t1;
    public Tile t2;
    public int cost;
};


public enum TileContent
{
    WALL = 0,
    FLOOR = 1,
    HOLE = 2,
    WATER = 3,
    SLOPE_D_L = 4,
    SLOPE_D_U = 5,
    SLOPE_D_R = 6,
    SLOPE_D_D = 7,
    SLOPE_U_L = 8,
    SLOPE_U_U = 9,
    SLOPE_U_R = 10,
    SLOPE_U_D = 11,
    UNDERSLOPE = 12
}
