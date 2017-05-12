using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    List<Tile[,]> floors;
    public List<Tile[,]> Floors { get { return floors; } }
    public GameObject mapContainer { get; set; }
}
