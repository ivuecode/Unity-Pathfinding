///-----------------------------------------------------------------
///   Class:          GraphController
///   Description:    Manages the data graph and collection of nodes
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class GraphController : MonoBehaviour
{
    [Header("Component References")]
    public GameObject nodeViewPrefab;                                                              // Reference to the world-spaceobject

    [Header("Graph Settings")]
    public Color startNodeColor;                                                                   // Color of the start node
    public Color endNodeColor;                                                                     // Color of the end node
    public Color blockedNodeColor;                                                                 // Color of the wall nodes
    public Color frontierColor;                                                                    // Color of the frontier nodes
    public Color exploredColor;                                                                    // Color of the previous explored nodes
    public Color pathColor;                                                                        // Color of the completed path nodes
    public Color defaultColor;

    [HideInInspector] public NodeView[,] nodeViews;                                                // 2D array of all the nodes (visual)
    [HideInInspector] public Node[,] nodes;                                                        // 2D array of all nodes (data)
    [HideInInspector] public int graphWidth;                                                       // Graph dimensions
    [HideInInspector] public int graphHeight;                                                      // Graph dimensions

    [Header("Private Variables")]
    private int[,] m_gridSize = new int[48, 24];                                                   // Base grid size
    private Vector2[] m_allDirections = { new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(-1f, 0f), new Vector2(0f, -1f), };  // directions for checking neighbors

    /// <summary>
    /// (short-hand function) Check (x,y) within the bounds of the Graph?
    /// </summary>
    private bool IsWithinBounds(int x, int y) => (x >= 0 && x < graphWidth && y >= 0 && y < graphHeight);

    /// <summary>
    /// (short-hand function) Returns a List of neighboring Nodes
    /// </summary>
    private List<Node> GetNeighbors(int x, int y) => GetNeighbors(x, y, nodes, m_allDirections);



    /// <summary>
    /// Initialize the Graph (data)
    /// </summary>
    public void Initialize()
    {
        // set the dimensions based on the array and initialize
        graphWidth = m_gridSize.GetLength(0);
        graphHeight = m_gridSize.GetLength(1);
        nodes = new Node[graphWidth, graphHeight];

        // at each (x,y) position in the array setup the node
        for (int y = 0; y < graphHeight; y++)
        {
            for (int x = 0; x < graphWidth; x++)
            {
                NodeType type = (NodeType)m_gridSize[x, y];
                Node newNode = new Node(x, y, type);
                nodes[x, y] = newNode;
                newNode.position = new Vector3(x, 0, y);
            }
        }
        UpdateAllNeighbours();
        CreateGraph();
        ResetGraph();
    }

    /// <summary>
    /// Create the graph grid (visual)
    /// </summary>
    private void CreateGraph()
    {
        // setup array of NodeViews
        nodeViews = new NodeView[graphWidth, graphHeight];

        foreach (Node n in nodes)
        {
            // create a NodeView for each corresponding Node
            GameObject instance = Instantiate(nodeViewPrefab, Vector3.zero, Quaternion.identity);
            NodeView nodeView = instance.GetComponent<NodeView>();
            nodeView.Init(n);

            // store each NodeView in the array
            nodeViews[n.xIndex, n.yIndex] = nodeView;
        }
    }

    /// <summary>
    /// setup the neighbor Nodes for each node in the array
    /// </summary>
    private void UpdateAllNeighbours()
    {
        for (int y = 0; y < graphHeight; y++)
        {
            for (int x = 0; x < graphWidth; x++)
            {
                if (nodes[x, y].nodeType != NodeType.Blocked)
                {
                    nodes[x, y].neighbors = GetNeighbors(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Returns a List of neighboring Nodes from (x,y) coordinate, array of Nodes and compass directions
    /// </summary>
    private List<Node> GetNeighbors(int x, int y, Node[,] nodeArray, Vector2[] directions)
    {
        List<Node> neighborNodes = new List<Node>();

        // in each direction vector...
        foreach (Vector2 dir in directions)
        {
            // find the (x,y) offset position
            int newX = x + (int)dir.x;
            int newY = y + (int)dir.y;

            // if the new position is within the graph and not blocked, add to List
            if (IsWithinBounds(newX, newY) && nodeArray[newX, newY] != null && nodeArray[newX, newY].nodeType != NodeType.Blocked)
            {
                neighborNodes.Add(nodeArray[newX, newY]);
            }
        }
        return neighborNodes;
    }

    /// <summary>
    /// Color a List of NodeViews, given a List of Nodes
    /// </summary>
    public void ColorNodes(List<Node> nodes, Color color)
    {
        foreach (Node n in nodes)
        {
            NodeView nodeView = nodeViews[n.xIndex, n.yIndex];
            nodeView.SetColorNode(color);
        }
    }

    /// <summary>
    /// Gets the approximate distance between nodes
    /// </summary>
    public float GetNodeDistance(Node source, Node target)
    {
        int dx = Mathf.Abs(source.xIndex - target.xIndex);
        int dy = Mathf.Abs(source.yIndex - target.yIndex);

        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);

        int straightSteps = max - min;

        return (1.4f * min + straightSteps);
    }

    /// <summary>
    /// Toggles the nodeType state (blocked / open)
    /// </summary>
    public void ToggleNodeState(int x, int y)
    {
        if (nodes[x, y].nodeType == NodeType.Open)
        {
            nodes[x, y].nodeType = NodeType.Blocked;
            nodeViews[x, y].SetColorNode(blockedNodeColor);
        }
        else
        {
            nodes[x, y].nodeType = NodeType.Open;
            nodeViews[x, y].SetColorNode(defaultColor);
        }
        UpdateAllNeighbours();
    }

    /// <summary>
    /// Reset the state of the graph
    /// </summary>
    public void ResetGraph()
    {
        // at each (x,y) position in the array setup the node
        for (int y = 0; y < graphHeight; y++)
        {
            for (int x = 0; x < graphWidth; x++)
            {
                nodeViews[x, y].SetColorNode(defaultColor);
                nodes[x, y].Reset();
                nodes[x, y].nodeType = NodeType.Open;
            }
        }
        UpdateAllNeighbours();
    }

    /// <summary>
    /// Keep the layout the same (used in the case where we want to changge algos)
    /// </summary>
    public void RerollGraph()
    {
        for (int y = 0; y < graphHeight; y++)
        {
            for (int x = 0; x < graphWidth; x++)
            {
                if (nodeViews[x, y].node.nodeType != NodeType.Blocked)
                {
                    nodeViews[x, y].SetColorNode(defaultColor);
                    nodeViews[x, y].node.Reset();
                }
            }
        }
    }
}