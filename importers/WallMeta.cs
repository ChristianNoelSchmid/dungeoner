
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers;

public class WallMeta : IMetaFile
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("backPanelRects")]
    [JsonConverter(typeof(Rect2ListConverter))]
    public List<Rect2> BackPanelRects { get; set; }

    private List<AtlasTexture> _textures;
    public Texture2D GetTexture(Vector2I position) {
        if(_textures == null) {
            _textures = Enumerable.Repeat<AtlasTexture>(null, BackPanelRects.Count).ToList();
        }
        int idx = position.GetHashCode() % BackPanelRects.Count;
        if(_textures[idx] == null) {
            _textures[idx] = new();
            var img = Image.LoadFromFile(FilePath);
            _textures[idx].Atlas = ImageTexture.CreateFromImage(img);
            _textures[idx].Region = BackPanelRects[idx];
        }

        return _textures[idx];
    }
}