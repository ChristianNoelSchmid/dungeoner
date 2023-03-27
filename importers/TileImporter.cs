using System.Collections.Generic;
using System.IO;
using Dungeoner.Collections;
using Godot;

namespace Dungeoner.Importers;

public partial class TileImporter : Node2D {
	private KeyCollection<Tile> _collection = new();

	public override void _Ready() => LoadAllTiles();

	public IEnumerable<Tile> GetAllMatchingMetas(string glob) => _collection.GetItems(glob);

	private void LoadAllTiles() {
        var tileMetas = Utilities.Load<TileMeta>("./assets/tiles");
        foreach ((string fileName, var tileMeta) in tileMetas) {
			if (File.Exists(tileMeta.FilePath)) {
				for(int i = 0; i < tileMeta.Parts.Length; i += 1) {
					var segment = tileMeta.Parts[i];
					if(!_collection.Insert(segment.Key, new Tile { Meta = tileMeta, Index = segment })) {
						GD.PushError(
							$"Duplicate token key `{segment.Key}` found. It will not be imported. " +
							$"Meta-file `{fileName}`, relative path: `{tileMeta.FilePath}`."
						);
					}
				}
			} else {
				GD.PushError(
					$"Could not find image associated with tile Meta-file `{fileName}`. " +
					$"Relative path: `{tileMeta.FilePath}`."
				);
			}
        }
	}
}
