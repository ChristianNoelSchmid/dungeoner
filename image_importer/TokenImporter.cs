using Dungeoner.Collections;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Dungeoner;

public partial class TokenImporter : Node2D
{
	private KeyCollection<ImageMeta> _imgCollection;

	PackedScene tokenScene = GD.Load<PackedScene>("res://tokens/token.tscn");

	public override void _Ready() {
		_imgCollection = new();
		LoadAllTokens();
    }

	public IEnumerable<ImageMeta> GetAllMatchingMetas(string glob) => _imgCollection.GetItems(glob);

	public ImageTexture GetTexture(ImageMeta imgMeta) {
		var img = new Image();
        img.Load(imgMeta.FilePath);

		return ImageTexture.CreateFromImage(img);
	}

    public Token GetToken(ImageMeta imgMeta) {
		var tex = GetTexture(imgMeta);

		var token = (Token)tokenScene.Instantiate();
		var sprite = token.GetChild<Sprite2D>(0);
		sprite.Texture = tex;
		sprite.Offset = new(-imgMeta.Pivot[0], -imgMeta.Pivot[1]);

		return token;
	}
	private void LoadAllTokens() {
        var imgMetas = GetImageMetas("./assets/tokens");
        foreach ((string fileName, var imgMeta) in imgMetas) {
            if (File.Exists(imgMeta.FilePath)) {
				if(_imgCollection.Insert(imgMeta.Key, imgMeta)) {

				} else {
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
	private List<(string, ImageMeta)> GetImageMetas(string rootPath) {
		List<(string, ImageMeta)> imgMetaFiles = new();
		var metaFileNames = Directory.EnumerateFiles(rootPath, "*.json", new EnumerationOptions { RecurseSubdirectories = true });
		foreach(var fileName in metaFileNames) {
			using var file = File.Open(fileName, FileMode.Open);
			using var fileReader = new StreamReader(file);
			string folderPath = Path.Combine(fileName.Split('/', '\\').SkipLast(1).ToArray());

			var imgMetas = JsonSerializer.Deserialize<List<ImageMeta>>(fileReader.ReadToEnd())
				.Select(imgMeta => imgMeta with { FilePath = Path.Combine(folderPath, imgMeta.FilePath) }).ToArray();

			imgMetaFiles.AddRange(Enumerable.Repeat(fileName, imgMetas.Length).Zip(imgMetas));
		}

		return imgMetaFiles;
	}
}
