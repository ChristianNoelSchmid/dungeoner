using Dungeoner.Collections;
using Godot;
using System.Collections.Generic;
using System.IO;

namespace Dungeoner.Importers;

public partial class TokenImporter : Node2D {
	private KeyCollection<TokenMeta> _imgCollection;

	PackedScene tokenScene = GD.Load<PackedScene>("res://tokens/token.tscn");

	public override void _Ready() {
		_imgCollection = new();
		LoadAllTokens();
    }

	public IEnumerable<TokenMeta> GetAllMatchingMetas(string glob) => _imgCollection.GetItems(glob);

	public ImageTexture GetTexture(TokenMeta imgMeta) {
		var img = new Image();
        img.Load(imgMeta.FilePath);

		return ImageTexture.CreateFromImage(img);
	}

    public Token GetToken(TokenMeta imgMeta) {
		var tex = GetTexture(imgMeta);

		var token = (Token)tokenScene.Instantiate();
		var sprite = token.GetChild<Sprite2D>(0);
		sprite.Texture = tex;
		sprite.Offset = new(-imgMeta.Pivot[0], -imgMeta.Pivot[1]);

		return token;
	}

	private void LoadAllTokens() {
        var imgMetas = Utilities.Load<TokenMeta>("./assets/tokens");
        foreach ((string fileName, var imgMeta) in imgMetas) {
            if (File.Exists(imgMeta.FilePath)) {
				if(!_imgCollection.Insert(imgMeta.Key, imgMeta)) {
					GD.PushError(
						$"Duplicate token key `{imgMeta.Key}` found. It will not be imported. " +
						$"Meta-file `{fileName}`, relative path: `{imgMeta.FilePath}`."
					);
				}
			} else {
				GD.PushError(
					$"Could not find image associated with `{imgMeta.Key}`. " +
					$"Meta-file `{fileName}`, relative path: `{imgMeta.FilePath}`."
				);
			}
        }
	}
}
