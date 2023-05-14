
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dungeoner.Importers.JsonConverters;
using Godot;

namespace Dungeoner.Importers;

public class TileMeta : IMetaFile
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("parts")]
    public TilePart[] Parts { get; set; } = Array.Empty<TilePart>();
}

public class TilePart
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("rects")]
    [JsonConverter(typeof(Rect2ListConverter))]
    public List<Rect2> Rects { get; set; } = new();
}

public class TileInstance
{
    public TileMeta Meta { get; set; }
    public TilePart Part { get; set; }

    public TileInstance(TileMeta meta, TilePart part)
    {
        Meta = meta;
        Part = part;
    }

    private List<AtlasTexture?>? _textures = null;
    public AtlasTexture GetTexture(Vector2I position)
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

            var image = Image.LoadFromFile(Meta.FilePath);
            _textures[idx]!.Atlas = ImageTexture.CreateFromImage(image);
            _textures[idx]!.Region = Part.Rects[idx];
        }

        return _textures[idx]!;
    }
}
