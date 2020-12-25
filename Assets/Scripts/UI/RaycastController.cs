///-----------------------------------------------------------------
///   Class:          RaycastController
///   Description:    Raycast from the camera to mouse position to interact
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    [Header("Component References")]
    public Camera sceneCamera;                                                                    // Reference to the main camera
    public UIController uIController;                                                             // Refernece to the UIController

    [Header("Private Variables")]
    private NodeView m_focusedNode;                                                                // Node we just click
    private NodeView m_currentNode;                                                                // Node under the mouse



    /// <summary>
    /// Update is called each frame
    /// </summary>
    void Update()
    {
        RaycastHit hit;
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            m_currentNode = hit.transform.GetComponent<NodeView>();

            if (Input.GetMouseButton(0))
            {
                if (m_focusedNode == null)
                {
                    m_focusedNode = m_currentNode;
                    uIController.OnInteract(m_focusedNode.node.xIndex, m_focusedNode.node.yIndex);
                }

                if (m_focusedNode != m_currentNode)
                {
                    uIController.OnInteract(m_currentNode.node.xIndex, m_currentNode.node.yIndex);
                    m_focusedNode = m_currentNode;
                }
            }
        }
    }
}