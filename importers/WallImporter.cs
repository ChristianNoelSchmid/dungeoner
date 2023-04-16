using System.Collections.Generic;
using System.IO;
using Dungeoner.Collections;
using Godot;

namespace Dungeoner.Importers;

public partial class WallImporter : Node {
	private KeyCollection<WallMeta> _collection = new();

	public IEnumerable<WallMeta> GetAllMatchingMetas(string glob) => _collection.GetItems(glob);

	public override void _Ready() {
		LoadAllWalls();
	}

	private void LoadAllWalls() {
        var wallMetas = Utilities.Load<WallMeta>("./assets/walls");
        foreach ((string fileName, var wallMeta) in wallMetas) {
            if (File.Exists(wallMeta.FilePath)) {
				if(!_collection.Insert(wallMeta.Key, wallMeta)) {
					GD.PushError(
						$"Duplicate token key `{wallMeta.Key}` found. It will not be imported. " +
						$"Meta-file `{fileName}`, relative path: `{wallMeta.FilePath}`."
					);
				}
			} else {
				GD.PushError(
					$"Could not find image associated with `{wallMeta.Key}`. " +
					$"Meta-file `{fileName}`, relative path: `{wallMeta.FilePath}`."
				);
			}
        }
	}
}
