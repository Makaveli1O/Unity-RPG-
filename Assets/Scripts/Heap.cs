using System;

/// <summary>
/// Heap abstract class
/// </summary>
/// <typeparam name="T"></typeparam>
public class Heap<T> where T : IHeapItem<T>
{
    T[]items;
    int currentItemCount;
    public Heap(int maxHeapSize){
        items = new T[maxHeapSize];
    }

    /// <summary>
    /// Add new item into the heap
    /// </summary>
    /// <param name="item">Item to be added</param>
    public void Add(T item){
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    /// <summary>
    /// Removing first element of the heap
    /// </summary>
    /// <returns>Removed item</returns>
    public T RemoveFirst(){
        T firstItem = items[0];
        currentItemCount--;
        //take item at the end of the heap and put it to first place
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    /// <summary>
    /// Sorting heap down
    /// </summary>
    /// <param name="item">Processed item</param>
    void SortDown(T item){
        while (true)
        {
            //indices of item's children
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            //does this item have at laest one child ( on the left )
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;
                //does have second child?
                if (childIndexRight < currentItemCount)
                {
                    //which have higher priority?
                    if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0){
                        swapIndex = childIndexRight;
                    }
                }

                //incorrect position
                if(item.CompareTo(items[swapIndex]) < 0){
                    Swap(item, items[swapIndex]);
                }else{//correct position
                    return; 
                }
            //no children   (correct pos) 
            }else{
                return;
            }
        }
    }

    /// <summary>
    /// Changing priority of given item within heap.
    /// </summary>
    /// <param name="item">Adjusted item</param>
    public void UpdateItem(T item){
        SortUp(item);
    }

    /// <summary>
    /// Number of items in heap accessor
    /// </summary>
    /// <value>Number of items</value>
    public int Count{
        get{
            return currentItemCount;
        }
    }

    /// <summary>
    /// Checks whenever heap contains given item
    /// </summary>
    /// <param name="item">Desired item to check.</param>
    /// <returns>Boolean if it's present or not</returns>
    public bool Contains(T item){
        return Equals(items[item.HeapIndex] ,item);
    }

    /// <summary>
    /// Sort heap upwards
    /// </summary>
    /// <param name="item">Processed item</param>
    void SortUp(T item){
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }else{
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    /// <summary>
    /// Swap two items within heap.
    /// </summary>
    /// <param name="itemA">First item</param>
    /// <param name="itemB">Second item</param>
    void Swap(T itemA, T itemB){
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

/// <summary>
/// Interface making sure that items are comparable
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHeapItem<T> : IComparable<T>{
    int HeapIndex{
        get;set;
    }
}
