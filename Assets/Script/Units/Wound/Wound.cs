using System;
using System.Collections.Generic;

public class Wound
{
    public Wound(WoundGravity gravity = WoundGravity.NONE)
    {
        this.gravity = gravity;
    }

    public WoundGravity gravity { get; set; }

    public enum WoundGravity
    {
        NONE,
        SURFACE,
        BLEEDING,
        SEVERE,
        BROKEN,
        REMOVED
    }
}

