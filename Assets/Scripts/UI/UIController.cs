///-----------------------------------------------------------------
///   Class:          UIController
///   Description:    Handles creating and starting the graph and pathfinder
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Component References")]
    public GraphController graph;                                                                  // Reference to the graph component
    public Pathfinder pathfinder;                                                                  // Referrence to the path finder
    public Slider timeStepSlider;                                                                  // Reference to the UI slider for setting time steps
    public Text timeStepValue;                                                                     // Reference to the time step value
    public Text completeTime;                                                                      // Refernece to the text for completion time
    public Text totalIterations;                                                                   // Reference to the total iterations
    public Dropdown algorithmDropdown;                                                             // Reference to the dropdown
    public Button resetButton;                                                                     // Refernece to the resetButton
    public Button startButton;                                                                     // Reference to the startButton

    [Header("Private Variables")]
    private int m_startNodeX;                                                                      // X coordinate of start Node
    private int m_startNodeY;                                                                      // Y coordinate of start Node
    private int m_endNodeX;                                                                        // X coordinate of end Node
    private int m_endNodeY;                                                                        // Y coordinate of end Node
    private float m_timeStep;                                                                      // Delay between iterations
    private int m_UIState;                                                                         // Internal tracking of the ui build state
    private bool hasStarted;                                                                       // Has this simulation started



    /// <summary>
    /// Initialize the graph
    /// </summary>
    private void Start()
    {
        graph.Initialize();
        OnSetTimestep();
    }

    /// <summary>
    /// Initialize the Pathfinder and begin the search
    /// </summary>
    public void OnStartSimulation()
    {
        graph.RerollGraph();
        Node startNode = graph.nodes[m_startNodeX, m_startNodeY];
        Node goalNode = graph.nodes[m_endNodeX, m_endNodeY];

        pathfinder.Initialize(startNode, goalNode);
        hasStarted = true;
        resetButton.interactable = false;
        startButton.interactable = false;
    }

    /// <summary>
    /// Sets the timesetp value from the UISlider
    /// </summary>
    public void OnSetTimestep()
    {
        m_timeStep = timeStepSlider.value / 10;
        timeStepValue.text = "Iterations per second: " + m_timeStep.ToString("F2");
        pathfinder.timeStep = m_timeStep;
    }

    /// <summary>
    /// Sets the pathfinder algorithm
    /// </summary>
    public void OnSetAlgorithm()
    {
        string algo = algorithmDropdown.options[algorithmDropdown.value].text;
        switch (algo)
        {
            case "Dijkstra":
                {
                    pathfinder.pathAlgo = PathFindingAlgo.Dijkstra;
                    break;
                }
            case "AStar":
                {
                    pathfinder.pathAlgo = PathFindingAlgo.AStar;
                    break;
                }
            case "Breadth First":
                {
                    pathfinder.pathAlgo = PathFindingAlgo.BreadthFirstSearch;
                    break;
                }
            case "Greedy Best First":
                {
                    pathfinder.pathAlgo = PathFindingAlgo.GreedyBestFirst;
                    break;
                }
        }
    }

    /// <summary>
    /// Resets the board
    /// </summary>
    public void OnResetBoard()
    {
        startButton.interactable = false;
        pathfinder.isComplete = false;
        hasStarted = false;
        m_UIState = 0;
        graph.ResetGraph();
    }

    /// <summary>
    /// Called from the pathfinder when a path is complete
    /// </summary>
    public void OnPathFinished()
    {
        resetButton.interactable = true;
        startButton.interactable = true;
    }

    /// <summary>
    /// Called when we interact with a node based on the state of the UI
    /// </summary>
    public void OnInteract(int xIndex, int yIndex)
    {
        if (hasStarted) return;
        switch (m_UIState)
        {
            case 0:
                {
                    graph.nodeViews[xIndex, yIndex].SetColorNode(graph.startNodeColor);
                    m_startNodeX = xIndex;
                    m_startNodeY = yIndex;
                    break;
                }
            case 1:
                {
                    graph.nodeViews[xIndex, yIndex].SetColorNode(graph.endNodeColor);
                    m_endNodeX = xIndex;
                    m_endNodeY = yIndex;
                    startButton.interactable = true;
                    break;
                }
            case 2:
                {
                    graph.ToggleNodeState(xIndex, yIndex);
                    break;
                }
        }
        m_UIState++;
        if (m_UIState >= 2) m_UIState = 2;
    }
}