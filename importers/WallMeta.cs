
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dungeoner.Importers.JsonConverters;
using Godot;

namespace Dungeoner.Importers;

public class WallMeta : IMetaFile
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("parts")]
    public List<WallPart> Parts { get; set; } = new();
}

public class WallPart
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("rects")]
    [JsonConverter(typeof(Rect2ListConverter))]
    public List<Rect2> Rects { get; set; } = new();

    [JsonPropertyName("drywall_color")]
    [JsonConverter(typeof(ColorConverter))]
    public Color DrywallColor { get; set; }
}

public record WallInstance(WallMeta Meta, WallPart Part)
{
    private List<AtlasTexture?>? _textures;
    public Texture2D GetTexture(Vector2I position)
    {
        if (_textures == null)
        {
            _textures = Enumerable.Repeat<AtlasTexture?>(null, Part.Rects.Count).ToList();
        }

        int idx = position.GetHashCode() % Part.Rects.Count;
        if (idx < 0) idx = Part.Rects.Count + idx;

        if (_textures[idx] == null)
        {
            _textures[idx] = new();
            var img = Image.LoadFromFile(Meta.FilePath);
            _textures[idx]!.Atlas = ImageTexture.CreateFromImage(img);
            _textures[idx]!.Region = Part.Rects[idx];
        }

        return _textures[idx]!;
    }
}