///-----------------------------------------------------------------
///   Class:          Node
///   Description:    Data class for the Node which implements the IComparable interface
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;

public enum NodeType { Open, Blocked };
public class Node : IComparable<Node>
{
    public int xIndex = -1;                                                                        // x and y index in the graph array
    public int yIndex = -1;                                                                        // x and y index in the graph array
    public float priority;                                                                         // Priority used to set place in queue
    public Vector3 position;                                                                       // (x,y,z) position in 3d space
    public Node previous = null;                                                                   // Referebce to preceding null in the current graph search
    public NodeType nodeType = NodeType.Open;
    public List<Node> neighbors = new List<Node>();                                                // List of neighbor Nodes
    public float distanceTraveled = Mathf.Infinity;                                                // Total distance traveled from the start Node



    /// <summary>
    /// Node constructor
    /// </summary>
    public Node(int x, int y, NodeType type)
    {
        xIndex = x;
        yIndex = y;
        nodeType = type;
    }

    /// <summary>
    /// Required by IComparable, method to compare this node with another Node based on priority
    /// </summary>
    public int CompareTo(Node other)
    {
        if (priority < other.priority) return -1;
        else if (priority > other.priority) return 1;
        else return 0;
    }

    /// <summary>
    /// Reset the state of this node
    /// </summary>
    public void Reset()
    {
        previous = null;
        priority = 0;
        distanceTraveled = Mathf.Infinity;
    }
}