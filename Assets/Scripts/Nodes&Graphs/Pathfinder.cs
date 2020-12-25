///-----------------------------------------------------------------
///   Class:          Pathfinder
///   Description:    Uses different algorithms to find a goal node 
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathFindingAlgo { Dijkstra, AStar, BreadthFirstSearch, GreedyBestFirst, };             // the various pathfinding algorithms
public class Pathfinder : MonoBehaviour
{
    [Header("Component References")]
    public GraphController graphController;                                                        // Refernece to the graph controller
    public UIController uIController;                                                              // Reference to the ui controller

    [Header("Private Variables")]
    private Node m_startNode;                                                                      // The start node
    private Node m_goalNode;                                                                       // The goal node
    private PriorityQueue<Node> m_frontierNodes;                                                   // The "open set" of Nodes that are next to be explored
    private List<Node> m_exploredNodes;                                                            // The "closed set" of Nodes that have been explored
    private List<Node> m_pathNodes;                                                                // The List of Nodes that make up our final path from start to goal

    [Header("PathFinding Properties")]
    [HideInInspector] public PathFindingAlgo pathAlgo;                                             // Active pathfinding mode/algorithm
    [HideInInspector] public float timeStep;                                                       // Speed of iterations
    [HideInInspector] public bool isComplete;                                                      // Is the search complete
    [HideInInspector] public int iterations;                                                       // Current number of iterations
    [HideInInspector] public float time;                                                           // Total time it took to complete



    /// <summary>
    /// Initialize the pathfinder
    /// </summary>
    public void Initialize(Node start, Node goal)
    {
        m_startNode = start;
        m_goalNode = goal;

        // our frontier begins with the start Node
        m_frontierNodes = new PriorityQueue<Node>();
        m_frontierNodes.Enqueue(start);
        m_exploredNodes = new List<Node>();
        m_pathNodes = new List<Node>();

        // reset all Nodes in the graph
        for (int x = 0; x < graphController.graphWidth; x++)
        {
            for (int y = 0; y < graphController.graphHeight; y++)
            {
                graphController.nodes[x, y].Reset();
            }
        }

        // setup starting values
        isComplete = false;
        iterations = 0;
        m_startNode.distanceTraveled = 0;
        StartCoroutine(SearchRoutine());
    }

    /// <summary>
    /// Updates the nodes on the graph to represent the current status of the algorithm
    /// </summary>
    private void UpdateGraphUI(Node start, Node goal)
    {
        // color frontier, explored and path Nodes
        if (m_frontierNodes != null) graphController.ColorNodes(m_frontierNodes.ToList(), graphController.frontierColor);
        if (m_exploredNodes != null) graphController.ColorNodes(m_exploredNodes, graphController.exploredColor);
        if (m_pathNodes != null && m_pathNodes.Count > 0) graphController.ColorNodes(m_pathNodes, graphController.pathColor);

        // color start NodeView and goal NodeView directly
        NodeView startNodeView = graphController.nodeViews[start.xIndex, start.yIndex];
        startNodeView.SetColorNode(graphController.startNodeColor);
        NodeView goalNodeView = graphController.nodeViews[goal.xIndex, goal.yIndex];
        goalNodeView.SetColorNode(graphController.endNodeColor);
    }

    /// <summary>
    /// Primary graph search routine
    /// </summary>
    private IEnumerator SearchRoutine()
    {
        // wait one frame
        float timeStart = Time.realtimeSinceStartup;
        yield return null;

        while (!isComplete && m_frontierNodes != null)
        {
            if (m_frontierNodes.Count > 0)
            {
                // get the next Node from the priority queue
                Node currentNode = m_frontierNodes.Dequeue();
                iterations++;
                time = (Time.realtimeSinceStartup - timeStart);
                uIController.totalIterations.text = "Total iterations: " + iterations;
                uIController.completeTime.text = "Elapsed time: " + time.ToString("F2") + " seconds";

                // mark this Node as explored
                if (!m_exploredNodes.Contains(currentNode)) m_exploredNodes.Add(currentNode);

                // expand the frontier based on our search mode
                if (pathAlgo == PathFindingAlgo.BreadthFirstSearch) ExpandFrontierBreadthFirst(currentNode);
                else if (pathAlgo == PathFindingAlgo.Dijkstra) ExpandFrontierDijkstra(currentNode);
                else if (pathAlgo == PathFindingAlgo.GreedyBestFirst) ExpandFrontierGreedyBestFirst(currentNode);
                else ExpandFrontierAStar(currentNode);

                // if the goal node is in the frontier
                if (m_frontierNodes.Contains(m_goalNode))
                {
                    m_pathNodes = GetPathNodes(m_goalNode);
                    isComplete = true;
                    uIController.OnPathFinished();
                }

                UpdateGraphUI(m_startNode, m_goalNode);
                yield return new WaitForSeconds(timeStep);
            }
            else
            {
                isComplete = true;
                uIController.OnPathFinished();
            }
        }
        time = (Time.realtimeSinceStartup - timeStart);
        uIController.OnPathFinished();
    }

    /// <summary>
    /// Expand the frontier nodes using Breadth First Search
    /// </summary>
    private void ExpandFrontierBreadthFirst(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                // if the current neighbor has not been explored and is not already part of the frontier
                if (!m_exploredNodes.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = graphController.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // create breadcrumb trail to neighbor node and set cumulative distance traveled
                    node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    node.neighbors[i].previous = node;

                    // add neighbor to explored Nodes, treat queue as if it were a first in-first out queue
                    node.neighbors[i].priority = m_exploredNodes.Count;
                    m_frontierNodes.Enqueue(node.neighbors[i]);
                }
            }
        }
    }

    /// <summary>
    /// Expand the frontier nodes using Dijkstra's algorithm
    /// </summary>
    private void ExpandFrontierDijkstra(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = graphController.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // if a shorter path exists to the neighbor via this node, re-route
                    if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
                    {
                        node.neighbors[i].previous = node;
                        node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    }

                    // if the current neighbor is not already part of the frontier
                    if (!m_frontierNodes.Contains(node.neighbors[i]))
                    {
                        // set the priority based on distance traveled from start Node and add to frontier
                        node.neighbors[i].priority = node.neighbors[i].distanceTraveled;
                        m_frontierNodes.Enqueue(node.neighbors[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Expand the frontier nodes using Greedy Best-First search
    /// </summary>
    private void ExpandFrontierGreedyBestFirst(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNodes.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = graphController.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // create breadcrumb trail to neighbor node and set cumulative distance traveled
                    node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    node.neighbors[i].previous = node;

                    // set the priority based on estimated distance to goal Node
                    node.neighbors[i].priority = graphController.GetNodeDistance(node.neighbors[i], m_goalNode);
                    m_frontierNodes.Enqueue(node.neighbors[i]);
                }
            }
        }
    }

    /// <summary>
    /// Expand the frontier nodes using AStar search
    /// </summary>
    private void ExpandFrontierAStar(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = graphController.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // if a shorter path exists to the neighbor via this node, re-route
                    if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
                    {
                        node.neighbors[i].previous = node;
                        node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    }

                    // if the neighbor is not part of the frontier, add this to the priority queue
                    if (!m_frontierNodes.Contains(node.neighbors[i]) && graphController != null)
                    {
                        // base priority, F score,  on G score (distance from start) + H score (estimated distance to goal)
                        float distanceToGoal = graphController.GetNodeDistance(node.neighbors[i], m_goalNode);
                        node.neighbors[i].priority = node.neighbors[i].distanceTraveled + distanceToGoal;

                        // add to priority queue using the F score
                        m_frontierNodes.Enqueue(node.neighbors[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate a list of path Nodes working backward from an end Node
    /// </summary>
    private List<Node> GetPathNodes(Node endNode)
    {
        List<Node> path = new List<Node>();
        if (endNode == null) return path;

        // follow the breadcrumb trail backward until we hit a node that has no previous node (usually the start Node)
        path.Add(endNode);
        Node currentNode = endNode.previous;

        while (currentNode != null)
        {
            // insert the previous node at the first position in the path
            path.Insert(0, currentNode);
            currentNode = currentNode.previous;
        }
        return path;
    }
}