using System;
using System.Collections;
using System.Collections.Generic;

namespace Dungeoner.Collections;

/// <summary>
/// A data structure that can quickly retrieve highest-priority
/// items, given a specific ordering.
/// </summary>
/// <typeparam name="T">The type of the items represented in the collection</typeparam>
public class Heap<T> : IEnumerable<T>
{
    private List<T> _items;
    private Func<T, T, int> _orderingFunction;

    public int Count => _items.Count;
    public bool IsEmpty => _items.Count == 0;

    /// <summary>
    /// Constructs the Heap with the given ordering function
    /// </summary>
    /// <param name="orderingFunction">The function used to compare the ordering of two items in the Heap</param>
    public Heap(Func<T, T, int> orderingFunction)
    {
        _items = new();
        _orderingFunction = orderingFunction;
    }

    /// <summary>
    /// Adds a new item to the Heap. This is an O(log(n)) operation
    /// </summary>
    /// <param name="item">The item to add to the Heap</param>
    public void Push(T item)
    {
        // Add the item to the end of the List
        _items.Add(item);
        int idx = _items.Count - 1;

        // Get the parent of the newly added item
        int parIdx = (idx - 1) / 2;

        // While the item isn't the root, and the item is higher priority than its parent,
        // swap the item and parent's positions
        while (idx != 0 && _orderingFunction(_items[idx], _items[parIdx]) > 0)
        {
            (_items[idx], _items[parIdx]) = (_items[parIdx], _items[idx]);
            idx = parIdx;
            parIdx = (idx - 1) / 2;
        }
    }

    /// <summary>
    /// Retrieves the topmost item without removing it, and returns whether successful
    /// This is an O(1) operation
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Peek(out T item)
    {
        item = default;
        if (IsEmpty) return false;
        item = _items[0];
        return true;
    }

    /// <summary>
    /// Removes and retrieves the topmost item and returns whether successful
    /// This is an O(log(n)) operation
    /// </summary>
    /// <param name="item">The item to remove and retrieve</param>
    /// <returns>Whether the item was removed, or false if the Heap is empty</returns>
    public bool Pop(out T item)
    {
        bool itemFound = Peek(out item);
        if (!itemFound) return false;

        // Assign the last item in the Heap to the top
        _items[0] = _items[_items.Count - 1];
        _items.RemoveAt(_items.Count - 1);

        // Now, traverse the top item down
        FixHeap(0);

        return true;
    }

    /// <summary>
    /// Removes the item from the Heap, if it exists.
    /// This is an O(n) operation.
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>Whether the item was found and removed</returns>
    public bool Remove(T item)
    {
        int idx = _items.IndexOf(item);
        if (idx == -1) return false;

        (_items[idx], _items[_items.Count - 1]) = (_items[_items.Count - 1], _items[idx]);
        _items.RemoveAt(_items.Count - 1);
        FixHeap(idx);

        return true;
    }

    // Performs an algorithm that rebalances the Heap from the given index
    // in the event that the item from the index was swapped during the removal
    // of an item
    private void FixHeap(int idx)
    {
        while (true)
        {
            // Index of the left child item
            int leftIdx = idx * 2 + 1;
            // Index of the right child item
            int rghtIdx = idx * 2 + 2;

            int selIdx = -1;

            // Choose the largest of the current node's children, if any children exist
            if (leftIdx < _items.Count && rghtIdx < _items.Count)
            {
                selIdx = (_orderingFunction(_items[leftIdx], _items[rghtIdx]) <= 0) ? rghtIdx : leftIdx;
            }
            else if (leftIdx < _items.Count)
            {
                selIdx = leftIdx;
            }
            else if (rghtIdx < _items.Count)
            {
                selIdx = rghtIdx;
            }

            // If a child was able to be selected, and it's a greater priority than
            // the current node, swap their positions and continue
            if (selIdx >= 0 && _orderingFunction(_items[idx], _items[selIdx]) < 0)
            {
                (_items[idx], _items[selIdx]) = (_items[selIdx], _items[idx]);
                idx = selIdx;
                // Otherwise, end the traversal
            }
            else
            {
                break;
            }
        }
    }

    /// <summary> 
    /// Enumerates over the items in the collection. Note that this is not necessarily
    /// in order of priority, although it is guaranteed that the first item will
    /// be the highest priority item.
    /// </summary>
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}