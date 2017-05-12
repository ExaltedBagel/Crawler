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

    public Tile(int x, int z)
    {
        mLinks = new List<Link>();
        Content = TileContent.WALL;
        Marked = MarkedFor.NOTHING;
        Coord = new Coordinate(x, z);
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
