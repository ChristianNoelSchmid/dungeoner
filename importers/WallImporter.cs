using System.Collections.Generic;
using System.IO;
using Dungeoner.Collections;
using Godot;

namespace Dungeoner.Importers;

public partial class WallImporter : Node
{
    private KeyCollection<WallInstance> _collection = new();

    public IEnumerable<WallInstance> GetAllMatchingMetas(string glob) => _collection.GetItems(glob);

    public override void _Ready()
    {
        LoadAllWalls();
    }

    private void LoadAllWalls()
    {
        var wallMetas = IO.Load<WallMeta>("./assets/walls");
        foreach ((string fileName, var wallMeta) in wallMetas)
        {
            if (File.Exists(wallMeta.FilePath))
            {
                foreach (var part in wallMeta.Parts)
                {
                    if (!_collection.Insert(part.Key, new(wallMeta, part)))
                    {
                        GD.PushError(
                            $"Duplicate token key `{part.Key}` found. It will not be imported. " +
                            $"Meta-file `{fileName}`, relative path: `{wallMeta.FilePath}`."
                        );
                    }
                }
            }
            else
            {
                GD.PushError(
                    $"Could not find image associated with tile Meta-file `{fileName}`. " +
                    $"Relative path: `{wallMeta.FilePath}`."
                );
            }
        }
    }
}
