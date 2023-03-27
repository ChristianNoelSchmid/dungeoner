
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers;

public class TileMeta : IMetaFile {
	[JsonPropertyName("filePath")]
	public string FilePath { get; set; }

	[JsonPropertyName("indices")]
	public TileIndex[] Parts { get; set; }
}

public class TileIndex {
	[JsonPropertyName("key")]
	public string Key { get; set; }

	[JsonPropertyName("rect")]
	[JsonConverter(typeof(Rect2Converter))]
	public Rect2 ImageBounds { get; set; }
}

public class Tile {
	public TileMeta Meta { get; set; }
	public TileIndex Index { get; set; }

	private AtlasTexture _texture;
	public AtlasTexture Texture {
		get {
			if(_texture == null) {
				var image = Image.LoadFromFile(Meta.FilePath);
				_texture = new();
				_texture.Atlas = ImageTexture.CreateFromImage(image);
				_texture.Region = Index.ImageBounds;
			}

			return _texture;
		}
	}
}
