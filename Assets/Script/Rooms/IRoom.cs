using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IRoom {

    public IUnit m_owner { get; set; }
    static List<IRoom> m_rooms;
    public List<IObject> m_objectsContained;
    public bool isPrivate { get; set; }
    public string roomName { get; set; }
    public Rectangle location;

    public IRoom(Rectangle location)
    {
        this.location = location;

        //Add all objects contained in location to the room.
        m_objectsContained = new List<IObject>();
        var tiles = MapGenerator.MapInstance.Floors[0];

        for (int i = location.left(); i <= location.right(); i++)
        {
            for(int j = location.bottom(); j <= location.top(); j++)
            {
                Debug.Log("Check tile " + i + ", " + j);
                if (tiles[i,j].ObjectContained != null)
                {
                    m_objectsContained.Add(tiles[i, j].ObjectContained);
                }
            }
        }

        Debug.Log("Room contained " + m_objectsContained.Count + " objects");

        RoomList().Add(this);
    }

    public bool IsAvailable()
    {
        return !isPrivate || m_owner == null;
    }

    //Returns the list of rooms
    static public List<IRoom> RoomList()
    {
        if (m_rooms == null)
            m_rooms = new List<IRoom>();

        return m_rooms;
    }

    static void AddRoom(IRoom newRoom)
    {
        m_rooms.Add(newRoom);
    }

}
