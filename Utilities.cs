
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dungeoner.GodotExtensions;
using Godot;

namespace Dungeoner;

public static class Utilities {
    public static IEnumerable<(string, T)> Load<T>(string rootPath) where T : IMetaFile {
		List<(string, T)> tMetaFiles = new();
		var metaFileNames = Directory.EnumerateFiles(rootPath, "*.json", new EnumerationOptions { RecurseSubdirectories = true });
		foreach(var fileName in metaFileNames) {
			using var file = File.Open(fileName, FileMode.Open);
			using var fileReader = new StreamReader(file);
			string folderPath = Path.Combine(fileName.Split('/', '\\').SkipLast(1).ToArray());

			var tMetas = JsonSerializer.Deserialize<T[]>(fileReader.ReadToEnd());
			if(tMetas == null) return Array.Empty<(string, T)>();

			foreach(var tMeta in tMetas) tMeta.FilePath = Path.Combine(folderPath, tMeta.FilePath);

			tMetaFiles.AddRange(Enumerable.Repeat(fileName, tMetas.Length).Zip(tMetas));
		}

		return tMetaFiles;
	}

	public static Vector2I ToGridPosition(this Vector2 position) => new (
		Mathf.RoundToInt(position.X / Constants.GRID_SIZE),
		Mathf.RoundToInt(position.Y / Constants.GRID_SIZE)
	);

	public static IEnumerable<(Vector2I, Vector2I)> GetEdgeNeighbors(Vector2I vector1, Vector2I vector2) {
        if(vector1.DirectionTo(vector2) == null) throw new ArgumentException("Vertices cannot be more than 1 space apart", nameof(vector2));
		return vector1.GetNeighbors().Select(n => (vector1, n))
			.Concat(vector2.GetNeighbors().Select(n => (vector2, n)))
			.Distinct().Where(pair => pair != (vector1, vector2) && pair != (vector2, vector1));
    }
}