
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers.JsonConverters;

public class Vector2IConverter : JsonConverter<Vector2I>
{
    public override Vector2I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int x, y;
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expecting number for x position of Rect");
        }
        else
        {
            x = reader.GetInt32();
        }
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expecting number for y position of Rect");
        }
        else
        {
            y = reader.GetInt32();
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expecting only 2 values for Vector2I (x, y)");
        }

        return new(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2I value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}