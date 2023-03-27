
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Godot;

namespace Dungeoner.Collections;

public class GridNeighborGraph<U> where U : class {
	private Dictionary<Vector2I, U?[]> _adjList = new();
	public bool TryGetEdges(Vector2I vertex, out U?[]? edges) {
		if(!_adjList.TryGetValue(vertex, out edges)) {
			return false;
		}       
		return true;
	}
	public bool TryGetEdge(Vector2I vertex1, Vector2I vertex2, out U? value) {
		Direction? dir = vertex1.DirectionTo(vertex2);              
		if(dir == null) throw new ArgumentException("Vertices cannot be more than 1 space apart", nameof(vertex2));

		if(!_adjList.TryGetValue(vertex1, out var edges) || edges[(int)dir] == null) {
			value = default;
			return false;
		}

		value = edges[(int)dir];
		return true; 
	}

	public void InsertEdge(Vector2I vertex1, Vector2I vertex2, U value) {
		var direction = vertex1.DirectionTo(vertex2);
		if(direction == null) throw new ArgumentException("Vertices cannot be more than 1 space apart", nameof(vertex2));

		if(!_adjList.ContainsKey(vertex1)) _adjList[vertex1] = Enumerable.Repeat<U?>(null, (int)Direction.Count).ToArray();
		if(!_adjList.ContainsKey(vertex2)) _adjList[vertex2] = Enumerable.Repeat<U?>(null, (int)Direction.Count).ToArray();

		_adjList[vertex1][(int)direction.Value] = value;
		_adjList[vertex2][(int)direction.Value.Reverse()] = value;
	}
	public bool RemoveEdge(Vector2I vertex1, Vector2I vertex2) {
		var direction = vertex1.DirectionTo(vertex2);
		if(direction == null) throw new ArgumentException("Vertices cannot be more than 1 space apart", nameof(vertex2));

		if(_adjList.TryGetValue(vertex1, out var list) && list[(int)direction] != null) {
			_adjList[vertex1][(int)direction] = null;
			_adjList[vertex2][(int)direction.Value.Reverse()] = null;

			if(_adjList[vertex1].All(a => a == null)) _adjList.Remove(vertex1);
			if(_adjList[vertex2].All(a => a == null)) _adjList.Remove(vertex2);

			return true;
		}
		return false;
	}
	public bool ContainsEdge(Vector2I vertex1, Vector2I vertex2) {
		if(_adjList.TryGetValue(vertex1, out var dirs)) {
			var dir = vertex1.DirectionTo(vertex2);
			if(dir == null) throw new ArgumentException("Vertices cannot be more than 1 space apart", nameof(vertex2));
			return dirs[(int)dir] != null;
		}
		return false;
	}
	public void Clear() => _adjList.Clear();
	public IEnumerable<(Vector2I from, Direction to, U value)> Edges() {
		HashSet<(Vector2I, Vector2I)> visitedEdges = new();
		foreach((var position, var edges) in _adjList) {
			for(Direction d = Direction.Up; d < Direction.Count; d += 1) {
				if(!visitedEdges.Contains((position, position.GetNeighbor(d))) && _adjList[position][(int)d] != null) {
					var neighbor = position.GetNeighbor(d);
					visitedEdges.Add((position, neighbor));
					visitedEdges.Add((neighbor, position));

					yield return (position, d, edges[(int)d]!);
				}
			}
		}
	}
}
