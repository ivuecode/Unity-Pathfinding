///-----------------------------------------------------------------
///   Class:          NodeView
///   Description:    Visual controller of the node
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///-----------------------------------------------------------------
using UnityEngine;

public class NodeView : MonoBehaviour
{
    [Header("Component Referneces")]
    public GameObject tile;
    public MeshRenderer materialRenderer;
    public Node node;



    /// <summary>
    /// Initilize the NodeView with the corresponding Node
    /// </summary>
    public void Init(Node _node)
    {
        // set the name, position and scale of this node
        gameObject.name = "Node (" + _node.xIndex + "," + _node.yIndex + ")";
        gameObject.transform.position = _node.position;
        tile.transform.localScale = new Vector3(1f - 0.1f, 1f, 1f - 0.1f);
        node = _node;
    }

    /// <summary>
    /// Set the color of this node (tile) geometry
    /// </summary>
    public void SetColorNode(Color color)
    {
        materialRenderer.material.color = color;
    }
}