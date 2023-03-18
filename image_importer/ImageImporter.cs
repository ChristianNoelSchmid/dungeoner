using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Dungeoner;

public partial class ImageImporter : Node2D
{
	[Export]
	private Node2D _worldNode;
	[Export]
	private SelectionTool _selectionTool;

	PackedScene tokenScene = GD.Load<PackedScene>("res://tokens/token.tscn");

	public override void _Ready() {
		var imgMetas = GetImageMetas("./assets/images");
		foreach(var imgMeta in imgMetas) {
			if(File.Exists(imgMeta.FilePath)) {
				var img = new Image();
				img.Load(imgMeta.FilePath);
				var rand = new Random();

				for(int i = 0; i < 1; i += 1) {
					var tex = ImageTexture.CreateFromImage(img);
					var token = (Token)tokenScene.Instantiate();
					var sprite = token.GetChild<Sprite2D>(0);
					sprite.Texture = tex;
					sprite.Offset = new(-imgMeta.Pivot[0], -imgMeta.Pivot[1]);
					_worldNode.AddChild(token);

					float randAngle = (float)(rand.NextDouble() * 2.0 * Math.PI);
					token.Position = new Vector2(Mathf.Cos(randAngle) * rand.Next(0, 200), Mathf.Sin(randAngle) * rand.Next(0, 200));

					_selectionTool.RegisterToken(token);
				}
			}
		}
	}

	private List<ImageMeta> GetImageMetas(string rootPath) {
		var metaFileNames = Directory.EnumerateFiles(rootPath, "*.json", new EnumerationOptions { RecurseSubdirectories = true });
		return metaFileNames.SelectMany(
			fileName => {
				using var file = File.Open(fileName, FileMode.Open);
				using var fileReader = new StreamReader(file);
				string folderPath = Path.Combine(fileName.Split('/', '\\').SkipLast(1).ToArray());

				return JsonSerializer.Deserialize<List<ImageMeta>>(fileReader.ReadToEnd())
					.Select(imgMeta => imgMeta with { FilePath = Path.Combine(folderPath, imgMeta.FilePath) });
			}
		).ToList();
	}
}
