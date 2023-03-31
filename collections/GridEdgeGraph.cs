
using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Godot;

namespace Dungeoner.Collections;

public class GridEdgeGraph<U> where U : class {
	private Dictionary<Vector2I, U?[]> _adjList = new();
	public bool TryGetEdges(Vector2I gridPosition, out U?[]? edgeValues) {
		if(!_adjList.TryGetValue(gridPosition, out edgeValues)) {
			return false;
		}
		return true;
	}
	public bool TryGetEdge(Vector2I gridPosition, Direction edge, out U? edgeValue) {
		if(!_adjList.TryGetValue(gridPosition, out var edges) || edges[(int)edge] == null) {
			edgeValue = default;
			return false;
		}

		edgeValue = edges[(int)edge];
		return true; 
	}

	public U? EdgeValueOrNull(Vector2I gridPosition, Direction edge) {
		if(!_adjList.TryGetValue(gridPosition, out var edges) || edges[(int)edge] == null) {
			return default;
		}
		return edges[(int)edge];
	}

	public void InsertEdge(Vector2I gridPosition, Direction edge, U edgeValue) {
		var adjacent = gridPosition.GetNeighbor(edge);

		if(!_adjList.ContainsKey(gridPosition)) _adjList[gridPosition] = Enumerable.Repeat<U?>(null, (int)Direction.Count).ToArray();
		if(!_adjList.ContainsKey(adjacent)) _adjList[adjacent] = Enumerable.Repeat<U?>(null, (int)Direction.Count).ToArray();

		_adjList[gridPosition][(int)edge] = edgeValue;
		_adjList[adjacent][(int)edge.Reverse()] = edgeValue;
	}
	public bool RemoveEdge(Vector2I gridPosition, Direction edge) {
		var adjacent = gridPosition.GetNeighbor(edge);

		if(_adjList.TryGetValue(gridPosition, out var list) && list[(int)edge] != null) {
			_adjList[gridPosition][(int)edge] = null;
			_adjList[adjacent][(int)edge.Reverse()] = null;

			if(_adjList[gridPosition].All(a => a == null)) _adjList.Remove(gridPosition);
			if(_adjList[adjacent].All(a => a == null)) _adjList.Remove(adjacent);

			return true;
		}
		return false;
	}
	public bool ContainsEdge(Vector2I vertex1, Direction edge) {
		if(_adjList.TryGetValue(vertex1, out var dirs)) {
			return dirs[(int)edge] != null;
		}
		return false;
	}
	public void Clear() => _adjList.Clear();
	public IEnumerable<(Vector2I gridPosition, Direction edge, U edgeValue)> Edges() {
		HashSet<(Vector2I, Direction)> visitedEdges = new();
		foreach((var position, var edges) in _adjList) {
			for(Direction d = Direction.Up; d < Direction.Count; d += 1) {
				if(!visitedEdges.Contains((position, d)) && _adjList[position][(int)d] != null) {
					var neighbor = position.GetNeighbor(d);
					visitedEdges.Add((position, d));
					visitedEdges.Add((neighbor, d.Reverse()));

					yield return (position, d, edges[(int)d]!);
				}
			}
		}
	}
}
