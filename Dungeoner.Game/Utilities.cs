
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
}