using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dungeoner.Importers;
using Dungeoner.Importers.JsonConverters;
using Godot;

namespace Dungeoner;

public class TokenMeta : IMetaFile
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("parts")]
    public TokenPart[] Parts { get; set; } = Array.Empty<TokenPart>();
}

public class TokenPart
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("rect")]
    [JsonConverter(typeof(Rect2Converter))]
    public Rect2 Rect { get; set; }

    [JsonPropertyName("pivot")]
    [JsonConverter(typeof(Vector2IConverter))]
    public Vector2I Pivot { get; set; }

    [JsonPropertyName("sort_height")]
    public int? SortHeight { get; set; } = -1;

    [JsonPropertyName("grid_size")]
    [JsonConverter(typeof(Vector2IConverter))]
    public Vector2I? GridSize { get; set; }
}

public class TokenInstance
{
    public TokenMeta Meta { get; set; }
    public TokenPart Part { get; set; }

    private AtlasTexture _texture;
    public AtlasTexture Texture
    {
        get
        {
            if (_texture == null)
            {
                var image = Image.LoadFromFile(Meta.FilePath);
                _texture = new();
                _texture.Atlas = ImageTexture.CreateFromImage(image);
                _texture.Region = Part.Rect;
            }

            return _texture;
        }
    }
}