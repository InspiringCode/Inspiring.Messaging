using System;
using System.Diagnostics;

namespace Inspiring.Messaging.Pipelines.Collections;

public class SlimList<T> {
    private const int DefaultCapacity = 4;
    private T[] _items = new T[0];

    public int Count { get; private set; }

    public T this[int index] {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(T item) {
        if (Count == _items.Length) Grow(Count + 1);
        _items[Count] = item;
        Count++;
    }

    public void Insert(int index, T item) {
        if (Count == _items.Length) Grow(Count + 1);

        if (index < Count)
            Array.Copy(_items, index, _items, index + 1, Count - index);

        _items[index] = item;

        Count++;
    }

    public void RemoveAt(int index) {
        Count--;

        if (index < Count)
            Array.Copy(_items, index + 1, _items, index, Count - index);

        _items[Count] = default!;
    }

    public T[] ToArray() {
        T[] items = new T[Count];
        Array.Copy(_items, items, Count);
        return items;
    }

    private void Grow(int capacity) {
        Debug.Assert(_items.Length < capacity);

        int c = _items.Length == 0 ?
            DefaultCapacity :
            2 * _items.Length;

        if (c < capacity)
            c = capacity;

        T[] newItems = new T[c];
        Array.Copy(_items, newItems, _items.Length);
        _items = newItems;
    }
}
