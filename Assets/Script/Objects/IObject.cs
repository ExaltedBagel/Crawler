using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IObject : MonoBehaviour {

    public IUnit m_owner { get; set; }
    public bool isPublic { get; set; }
    public Coordinate location;

    public void SetInTile(Tile t)
    {
        location = t.Coord;
        transform.position = new Vector3(location.x, 0.5f, location.z);
        t.ObjectContained = this;
    }

    public abstract void Use(IUnit user);
    public abstract void BeClaimedBy(IUnit user);

}
