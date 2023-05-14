using System.Collections.Generic;
using System.IO;
using Dungeoner.Collections;
using Godot;

namespace Dungeoner.Importers;

public partial class TileImporter : Node2D
{
    private KeyCollection<TileInstance> _collection = new();

    public override void _Ready() => LoadAllTiles();

    public IEnumerable<TileInstance> GetAllMatchingMetas(string glob) => _collection.GetItems(glob);

    private void LoadAllTiles()
    {
        var tileMetas = IO.Load<TileMeta>("./assets/tiles");
        foreach ((string fileName, var tileMeta) in tileMetas)
        {
            if (File.Exists(tileMeta.FilePath))
            {
                for (int i = 0; i < tileMeta.Parts.Length; i += 1)
                {
                    var part = tileMeta.Parts[i];
                    if (!_collection.Insert(part.Key, new TileInstance(tileMeta, part)))
                    {
                        GD.PushError(
                            $"Duplicate token key `{part.Key}` found. It will not be imported. " +
                            $"Meta-file `{fileName}`, relative path: `{tileMeta.FilePath}`."
                        );
                    }
                }
            }
            else
            {
                GD.PushError(
                    $"Could not find image associated with tile Meta-file `{fileName}`. " +
                    $"Relative path: `{tileMeta.FilePath}`."
                );
            }
        }
    }
}
