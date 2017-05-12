using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Edge
{
    public bool isInner { get; }
    public bool isVertical { get; }
    public bool isReversed { get; }
    public Side side { get; }
    public Coordinate start { get; set; }
    public Coordinate end { get; set; }

    public Edge(Coordinate start, Coordinate end, Side side, bool isInner = true)
    {
        this.start = start;
        this.end = end;
        this.isInner = isInner;
        this.side = side;
        this.isVertical = side.Equals(Side.LEFT) || side.Equals(Side.RIGHT);
    }    

    static public bool EdgesOverlap(Edge e1, Edge e2)
    {
        //Are they the same orientation?
        if (e1.isVertical ^ e2.isVertical)
        {
            return false;
        }
        else
        {
            //Treat Vertical case
            if(e1.isVertical)
            {
                //Not aligned means they dont intersect
                if (e1.start.x != e2.start.x)
                    return false;
                //They might intersect
                else
                {
                    int e1Min = Math.Min(e1.start.z, e1.end.z);
                    int e1Max = Math.Max(e1.start.z, e1.end.z);
                    int e2Min = Math.Min(e2.start.z, e2.end.z);
                    int e2Max = Math.Max(e2.start.z, e2.end.z);

                    if (e1Min > e2Max || e2Min > e1Max)
                    {
                        return false;
                    }
                    else
                        return true;
                }                
            }
            //Treat horizontal case
            else
            {
                //Not aligned means they dont intersect
                if (e1.start.z != e2.start.z)
                    return false;
                //They might intersect
                else
                {
                    int e1Min = Math.Min(e1.start.x, e1.end.x);
                    int e1Max = Math.Max(e1.start.x, e1.end.x);
                    int e2Min = Math.Min(e2.start.x, e2.end.x);
                    int e2Max = Math.Max(e2.start.x, e2.end.x);

                    if (e1Min > e2Max || e2Min > e1Max)
                    {
                        return false;
                    }
                    else
                        return true;
                }
            }
        }
    }

    static public List<Edge> SubtractEdges(Edge e1, Edge e2)
    {
        List<Edge> l = new List<Edge>();
        if(!EdgesOverlap(e1, e2))
        {
            //l.Add(e1); l.Add(e2);
            return l;
        }
        else
        {
            int e1Min;
            int e1Max;
            int e2Min;
            int e2Max;
            //Find point of overlap
            if (e1.isVertical)
            {
                e1Min = Math.Min(e1.start.z, e1.end.z);
                e1Max = Math.Max(e1.start.z, e1.end.z);
                e2Min = Math.Min(e2.start.z, e2.end.z);
                e2Max = Math.Max(e2.start.z, e2.end.z);

                //Continuation of an edge
                if(e1Max == e2Min) 
                {
                    Coordinate e1Start = new Coordinate(e2.start.x, e2Max);
                    Coordinate e1End = new Coordinate(e2.start.x, e1Min);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                    return l;
                }
                else if (e2Max == e1Min)
                {
                    Coordinate e1Start = new Coordinate(e2.start.x, e2Min);
                    Coordinate e1End = new Coordinate(e2.start.x, e1Max);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                    return l;
                }

                if (e1Max != e2Max)
                {
                    Coordinate e1Start = new Coordinate(e2.start.x, e2Max);
                    Coordinate e1End = new Coordinate(e2.start.x, e1Max);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                }
                if (e1Min != e2Min)
                {
                    Coordinate e2Start = new Coordinate(e2.start.x, e2Min);
                    Coordinate e2End = new Coordinate(e2.start.x, e1Min);
                    l.Add(new Edge(e2Start, e2End, e2.side));
                }

                return l;                
            }        
            else
            {
                e1Min = Math.Min(e1.start.x, e1.end.x);
                e1Max = Math.Max(e1.start.x, e1.end.x);
                e2Min = Math.Min(e2.start.x, e2.end.x);
                e2Max = Math.Max(e2.start.x, e2.end.x);

                //Continuation of an edge
                if (e1Max == e2Min)
                {
                    Coordinate e1Start = new Coordinate(e2Max , e2.start.z);
                    Coordinate e1End = new Coordinate(e1Min, e2.start.z);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                    return l;
                }
                else if (e2Max == e1Min)
                {
                    Coordinate e1Start = new Coordinate(e2Min, e2.start.z);
                    Coordinate e1End = new Coordinate(e1Max, e2.start.z);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                    return l;
                }

                if (e1Max != e2Max)
                {
                    Coordinate e1Start = new Coordinate(e2Max, e2.start.z);
                    Coordinate e1End = new Coordinate(e1Max, e2.start.z);
                    l.Add(new Edge(e1Start, e1End, e2.side));
                }
                if(e1Min != e2Min)
                {
                    Coordinate e2Start = new Coordinate(e2Min, e2.start.z);
                    Coordinate e2End = new Coordinate(e1Min, e2.start.z);
                    l.Add(new Edge(e2Start, e2End, e2.side));
                }

                return l;               
                
            }
        }
    }

    static public List<Edge> ExtractEdges(Rectangle r, bool isInner = true)
    {
        List<Edge> e = new List<Edge>();
        //Get left edge
        e.Add(new Edge(r.topLeft, new Coordinate(r.left(), r.bottom() - 1), Side.LEFT, isInner));

        //Get right edge
        e.Add(new Edge(new Coordinate(r.right() + 1, r.bottom() - 1), new Coordinate(r.right() + 1, r.top()), Side.RIGHT, isInner));

        //Get top edge
        e.Add(new Edge(r.topLeft, new Coordinate(r.right() + 1, r.top()), Side.UP, isInner));

        //Get bottom edge
        e.Add(new Edge(new Coordinate(r.left(), r.bottom() - 1), new Coordinate(r.right() + 1, r.bottom() - 1), Side.DOWN, isInner));

        return e;
    }

    public void Print()
    {
        Debug.Log(start.x + ", " + start.z + " - " + end.x + ", " + end.z);
    }

    public enum Side
    {
        LEFT,
        UP,
        RIGHT,
        DOWN
    }
}
