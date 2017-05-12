using System;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle
{
    public Rectangle(int top, int left, int bottom, int right)
    {
        this.topLeft = new Coordinate(left, top);
        width = right - left;
        height = top - bottom;
    }

    public Rectangle(Coordinate topLeft, Coordinate bottomRight)
    {
        this.topLeft = topLeft;
        width = bottomRight.x - topLeft.x;
        height = topLeft.z - bottomRight.z;
    }

    public Coordinate topLeft { get; set; }
    public int top() { return topLeft.z; }
    public int left() { return topLeft.x; }
    public int right() { return topLeft.x + width; }
    public int bottom() { return topLeft.z - height; }

    public int width { get; set; }
    public int height { get; set; }
    public int Area() { return width * height; }

    public bool IsIntersecting(Rectangle other)
    {
        //Check for line intersections - Preuve par absurde + Morgan
        if( left() < other.right() && right() > other.left() &&
            top() > other.bottom() && bottom() < other.top())
        {
            return true;
        }

        return false;
    }

    public bool IsAdjacent(Rectangle other)
    {
        List<Edge> r1 = Edge.ExtractEdges(this);
        List<Edge> r2 = Edge.ExtractEdges(other);

        foreach (Edge e1 in r1)
        {
            foreach (Edge e2 in r2)
            {
                if (Edge.EdgesOverlap(e1, e2))
                    return true;
            }
        }
        return false;
    }

    public bool Contains(Coordinate coord)
    {
        return (coord.x > left() && coord.x < right() && coord.z > bottom() && coord.z < top());
    }

    public bool Contains(int x, int z)
    {
        return (x > left() && x < right() && z > bottom() && z < top());
    }

    public void Print()
    {
        Debug.Log(" (x, z) - (" + left() + ", " + top() +
                "), (" + right() + ", " + top() +
                "), (" + left() + ", " + bottom() +
                "), (" + right() + ", " + bottom() + ")");
    }
}

