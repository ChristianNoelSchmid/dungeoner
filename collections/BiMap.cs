
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Collections;

public class BiMap<T, U> : IEnumerable<(T, U)>
    where T : notnull
    where U : notnull
{
    private Dictionary<T, U> _tToU = new();
    private Dictionary<U, T> _uToT = new();

    public U this[T key]
    {
        get => _tToU[key];
        set
        {
            _tToU[key] = value;
            _uToT[value] = key;
        }
    }
    public T this[U key]
    {
        get => _uToT[key];
        set
        {
            _uToT[key] = value;
            _tToU[value] = key;
        }
    }

    public IEnumerable<T> Set1 => _uToT.Values;
    public IEnumerable<U> Set2 => _tToU.Values;

    public bool ContainsKey(T key) => _tToU.ContainsKey(key);
    public bool ContainsKey(U key) => _uToT.ContainsKey(key);

    public bool TryGetValue(T key, out U? value) => _tToU.TryGetValue(key, out value);
    public bool TryGetValue(U key, out T? value) => _uToT.TryGetValue(key, out value);

    public U? GetValueOrDefault(T key) => _tToU.GetValueOrDefault(key);
    public T? GetValueOrDefault(U key) => _uToT.GetValueOrDefault(key);

    public void Add(T key, U value)
    {
        _tToU.Add(key, value);
        _uToT.Add(value, key);
    }
    public void Add(U key, T value)
    {
        _uToT.Add(key, value);
        _tToU.Add(value, key);
    }

    public bool Remove(T key)
    {
        U? value = GetValueOrDefault(key);
        if (value == null) return false;
        else
        {
            _uToT.Remove(value);
            return _tToU.Remove(key);
        }
    }

    public bool Remove(U key)
    {
        T? value = GetValueOrDefault(key);
        if (value == null) return false;
        else
        {
            _tToU.Remove(value);
            return _uToT.Remove(key);
        }
    }

    public IEnumerator<(T, U)> GetEnumerator() => _tToU.Select(pair => (pair.Key, pair.Value)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator<(T, U)>)GetEnumerator();
}