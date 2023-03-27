
using Microsoft.Extensions.FileSystemGlobbing;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Collections;

public class KeyCollection<T> : IComparable<KeyCollection<T>> {
    private string _key;
    private List<(string key, T value)> _entries;
    private List<KeyCollection<T>> _subCollections;
    private Comparer<(string, T)> _entriesComparer = Comparer<(string, T)>.Create((a, b) => a.Item1.CompareTo(b.Item1));

    public KeyCollection() {
        _key = string.Empty;
        _entries = new();
        _subCollections = new();
    }

    private KeyCollection(string key) {
        _key = key; 
        _entries = new();
        _subCollections = new();
    }

    public bool Insert(string keyPath, T value) => Insert(keyPath, 0, value);
    private bool Insert(string keyPath, int pathIndex, T value) {
        int nextIdx = keyPath[pathIndex..].IndexOf("/");
        if(nextIdx == -1) {
            int idx = _entries.BinarySearch((keyPath[pathIndex..], value), _entriesComparer);
            if(idx < 0) _entries.Insert(~idx, (keyPath[pathIndex..], value));
            return idx < 0;
        } else {
            nextIdx += pathIndex;
            var newCollection = new KeyCollection<T>(keyPath[pathIndex..nextIdx]);
            int idx = _subCollections.BinarySearch(newCollection);
            if(idx < 0) {
                idx = ~idx;
                _subCollections.Insert(idx, newCollection);
            }

            return _subCollections[idx].Insert(keyPath, nextIdx + 1, value);
        }
    }

    public IEnumerable<T> GetItems(string globKey) {
        var matcher = new Matcher();
        matcher.AddInclude(globKey);
        return GetItems(globKey, Array.Empty<string>(), 0, matcher, false);
    }
    private IEnumerable<T> GetItems(
        string globKey, 
        IEnumerable<string> currentPath,
        int pathIdx, 
        Matcher matcher, 
        bool wildcard
    ) {
        string keyPath = string.Join('/', currentPath);
        foreach(var entry in _entries) {
            if(matcher.Match($"{keyPath}/{entry.key}").HasMatches) {
                yield return entry.value; 
            }
        }
        
        int nextIdx = globKey[pathIdx..].IndexOf("/");
        var subMatcher = new Matcher();

        if(nextIdx != -1) {
            nextIdx += pathIdx;
            if(globKey[pathIdx..nextIdx].Trim() == "**") {
                wildcard = true;
            }
            subMatcher.AddInclude($"{keyPath}/{globKey[pathIdx..nextIdx]}");
        }
        // Match everything if there's a wildcard
        if(wildcard) { 
            subMatcher.AddInclude("**/*");
        }

        foreach(var collection in _subCollections) {
            var newPath = currentPath.Append(collection._key);
            if(subMatcher.Match(string.Join('/', newPath)).HasMatches) {
                var items = collection.GetItems(globKey, newPath, nextIdx + 1, matcher, wildcard);
                foreach(var item in items) yield return item;
            } 
        }
    }


    public override bool Equals(object? obj) => obj is not null && obj is KeyCollection<T> col && col._key == _key;
    public override int GetHashCode() => _key.GetHashCode();

    public int CompareTo(KeyCollection<T>? other) => _key.CompareTo(other?._key);
}