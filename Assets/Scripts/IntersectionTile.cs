using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTile : MonoBehaviour
{
    [Space(10), Header("Neighbors")]
    public IntersectionTile UpNeighbor;
    public IntersectionTile LeftNeighbor;
    public IntersectionTile DownNeighbor;
    public IntersectionTile RightNeighbor;
    
    [Space(10), Header("Neighbor Directions")]
    public Vector3 UpDirection;
    public Vector3 LeftDirection;
    public Vector3 DownDirection;
    public Vector3 RightDirection;

    [Space(10), Header("Neighbor Collections")]
    public IntersectionTile[] Neighbors;
    public Vector3[] NeighborDirections;

    [Space(10), Header("Portal Configuration")]
    public bool IsPortal;
    public IntersectionTile OppositePortal;

    private void Awake()
    {
        UpDirection = UpNeighbor != null 
            ? (UpNeighbor.transform.localPosition - transform.localPosition).normalized
            : Vector3.zero;

        LeftDirection = LeftNeighbor != null
            ? (LeftNeighbor.transform.localPosition - transform.localPosition).normalized
            : Vector3.zero;

        DownDirection = DownNeighbor != null
            ? (DownNeighbor.transform.localPosition - transform.localPosition).normalized
            : Vector3.zero;

        RightDirection = RightNeighbor != null
            ? (RightNeighbor.transform.localPosition - transform.localPosition).normalized
            : Vector3.zero;

        Neighbors = new[] { UpNeighbor, LeftNeighbor, DownNeighbor, RightNeighbor };
        NeighborDirections = new[] { UpDirection, LeftDirection, DownDirection, RightDirection };
    }
}
