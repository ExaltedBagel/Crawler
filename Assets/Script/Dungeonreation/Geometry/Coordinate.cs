using System;
using System.Collections.Generic;

public class Coordinate
{
    public int x { get; set; }
    public int z { get; set; }

    public Coordinate(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool IsLeftOf(Coordinate other)
    {
        return x < other.x;
    }

    public bool IsLeftOrEqualOf(Coordinate other)
    {
        return x <= other.x;
    }

    public bool IsValidLeft(Coordinate other)
    {
        return x + 1 < other.x;
    }

    public bool IsRightOf(Coordinate other)
    {
        return x > other.x;
    }

    public bool IsRightOrEqualOf(Coordinate other)
    {
        return x >= other.x;
    }

    public bool IsValidRight(Coordinate other)
    {
        return x - 1 > other.x;
    }

    public bool IsUnder(Coordinate other)
    {
        return z < other.z;
    }

    public bool IsUnderOrEqual(Coordinate other)
    {
        return z <= other.z;
    }

    public bool IsValidUnder(Coordinate other)
    {
        return z + 1 < other.z;
    }

    public bool IsOver(Coordinate other)
    {
        return z > other.z;
    }

    public bool IsOverOrEqual(Coordinate other)
    {
        return z >= other.z;
    }

    public bool IsValidOver(Coordinate other)
    {
        return z - 1 < other.z;
    }

}

