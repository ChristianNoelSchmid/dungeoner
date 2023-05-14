using Dungeoner.Collections;
using Godot;
using System.Collections.Generic;
using System.IO;

namespace Dungeoner.Importers;

public partial class TokenImporter : Node2D
{
    private KeyCollection<TokenInstance> _imgCollection = new();

    PackedScene tokenScene = GD.Load<PackedScene>("res://tokens/token.tscn");

    public override void _Ready()
    {
        LoadAllTokens();
    }

    public IEnumerable<TokenInstance> GetAllMatchingMetas(string glob) => _imgCollection.GetItems(glob);

    public Token GetToken(TokenInstance instance)
    {
        var token = (Token)tokenScene.Instantiate();
        token.Instance = instance;
        return token;
    }

    private void LoadAllTokens()
    {
        var imgMetas = IO.Load<TokenMeta>("./assets/tokens");
        foreach ((string fileName, var imgMeta) in imgMetas)
        {
            if (File.Exists(imgMeta.FilePath))
            {
                for (int i = 0; i < imgMeta.Parts.Length; i += 1)
                {
                    // If no GridSize if given, base it off of the rect size
                    if (imgMeta.Parts[i].GridSize == null)
                    {
                        imgMeta.Parts[i].GridSize = new(
                            Mathf.RoundToInt(imgMeta.Parts[i].Rect.Size.X / Constants.GRID_SIZE),
                            Mathf.RoundToInt(imgMeta.Parts[i].Rect.Size.Y / Constants.GRID_SIZE)
                        );
                    }
                    if (imgMeta.Parts[i].SortHeight == -1)
                    {
                        imgMeta.Parts[i].SortHeight = (int)imgMeta.Parts[i].Rect.End.Y;
                    }

                    if (!_imgCollection.Insert(imgMeta.Parts[i].Key, new TokenInstance
                    {
                        Meta = imgMeta,
                        Part = imgMeta.Parts[i]
                    }))
                    {
                        GD.PushError(
                            $"Duplicate token key `{imgMeta.Parts[i].Key}` found. It will not be imported. " +
                            $"Meta-file `{fileName}`, relative path: `{imgMeta.FilePath}`."
                        );
                    }
                }
            }
            else
            {
                GD.PushError(
                    $"Could not find image associated in meta-file `{fileName}`. Relative path: `{imgMeta.FilePath}`."
                );
            }
        }
    }
}
