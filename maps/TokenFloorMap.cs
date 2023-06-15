using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeoner.Maps;

public partial class TokenFloorMap : Node2D 
{
    private List<Token> _tokens = new();

    public void AddToken(Token token) 
    {
        AddChild(token);

        // Insert the Token in the very front
        token.ZIndex = (_tokens.LastOrDefault()?.ZIndex ?? -1) + 1;
        _tokens.Add(token);
    }

    public void RemoveToken(Token token) 
    {
        RemoveChild(token);
        _tokens.Remove(token);
    }

    public void MoveForward(Token token) 
    {
        int i = FindIndex(token); 
        if(i == _tokens.Count - 1) return;

        (_tokens[i].ZIndex, _tokens[i + 1].ZIndex) = (_tokens[i + 1].ZIndex, _tokens[i].ZIndex);
        (_tokens[i], _tokens[i + 1]) = (_tokens[i + 1], _tokens[i]);
    }

    public void MoveToFront(Token token) 
    {
        int i = FindIndex(token);
        if(i == _tokens.Count - 1) return;

        _tokens[i].ZIndex = _tokens[^1].ZIndex + 1;
        _tokens.RemoveAt(i);
        _tokens.Add(token);
    }

    public void MoveBackward(Token token) 
    {
        int i = FindIndex(token); 
        if(i == 0) return;

        (_tokens[i].ZIndex, _tokens[i - 1].ZIndex) = (_tokens[i - 1].ZIndex, _tokens[i].ZIndex);
        (_tokens[i], _tokens[i - 1]) = (_tokens[i - 1], _tokens[i]);
    }

    public void MoveToBack(Token token) 
    {
        int i = FindIndex(token);
        if(i == 0) return;

        _tokens[i].ZIndex = _tokens[0].ZIndex - 1;
        _tokens.RemoveAt(i);
        _tokens.Insert(i, token);
    }

    private int FindIndex(Token token) 
    {
        var comparer = Comparer<Token>.Create((a, b) => a.ZIndex.CompareTo(b.ZIndex));
        int index = _tokens.BinarySearch(token, comparer);

        if(index < 0) throw new IndexOutOfRangeException("Could not find token in floor map");

        return index;
    }
}