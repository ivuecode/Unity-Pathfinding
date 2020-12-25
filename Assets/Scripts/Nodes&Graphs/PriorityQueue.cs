///-----------------------------------------------------------------
///   Class:          PriorityQueue
///   Description:    Priority queue implemented using generic list with a binary heap
///   Author:         Lee
///   GitHub:         https://github.com/ivuecode
///   Notes:          Implementation: https://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
///-----------------------------------------------------------------
using System;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{
    // List of items in our queue
    private List<T> m_data;

    /// <summary>
    /// Number of items currently in queue
    /// </summary>
    public int Count => m_data.Count;

    /// <summary>
    /// Check if the item contained in the data List?
    /// </summary>
    public bool Contains(T item) => m_data.Contains(item);

    /// <summary>
    /// Return the data as a generic List
    /// </summary>
    public List<T> ToList() => m_data;

    /// <summary>
    /// Constructor
    /// </summary>
    public PriorityQueue() => m_data = new List<T>();

    /// <summary>
    /// Look at the first item without dequeuing
    /// </summary>
    public T Peek()
    {
        T frontItem = m_data[0];
        return frontItem;
    }

    /// <summary>
    /// Add an item to the queue and sort using a min binary heap
    /// </summary>
    public void Enqueue(T item)
    {
        m_data.Add(item);
        int childindex = m_data.Count - 1;

        // sort using a min binary heap
        while (childindex > 0)
        {
            // find the parent position in the heap
            int parentindex = (childindex - 1) / 2;

            // if parent and child are already sorted, stop sorting
            if (m_data[childindex].CompareTo(m_data[parentindex]) >= 0) break;

            T tmp = m_data[childindex];
            m_data[childindex] = m_data[parentindex];
            m_data[parentindex] = tmp;

            // move up one level in the heap and repeat until sorted
            childindex = parentindex;
        }
    }

    /// <summary>
    /// Remove an item from queue and keep it sorted using a min binary heap
    /// </summary>
    public T Dequeue()
    {
        int lastindex = m_data.Count - 1;
        T frontItem = m_data[0];
        m_data[0] = m_data[lastindex];
        m_data.RemoveAt(lastindex);
        lastindex--;
        int parentindex = 0;

        // sort using binary heap
        while (true)
        {
            // left child
            int childindex = parentindex * 2 + 1;

            // if there is no left child, stop sorting
            if (childindex > lastindex) break;

            // right child
            int rightchild = childindex + 1;

            // if the value of the right child is less than the left child, switch to the right branch of the heap
            if (rightchild <= lastindex && m_data[rightchild].CompareTo(m_data[childindex]) < 0) childindex = rightchild;

            // if the parent and child are already sorted, then stop sorting
            if (m_data[parentindex].CompareTo(m_data[childindex]) <= 0) break;

            // if not, then swap the parent and child
            T tmp = m_data[parentindex];
            m_data[parentindex] = m_data[childindex];
            m_data[childindex] = tmp;

            // move down the heap onto the child's level and repeat until sorted
            parentindex = childindex;
        }
        return frontItem;
    }
}