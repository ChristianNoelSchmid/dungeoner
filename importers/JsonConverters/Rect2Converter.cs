
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers.JsonConverters;

public class Rect2Converter : JsonConverter<Rect2>
{
    public override Rect2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int x, y, width, height;
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
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expecting number for width of Rect");
        }
        else
        {
            width = reader.GetInt32();
        }
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expecting number for height of Rect");
        }
        else
        {
            height = reader.GetInt32();
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expecting only 4 values for Rect (x, y, width, height)");
        }

        return new(x, y, width, height);
    }

    public override void Write(Utf8JsonWriter writer, Rect2 value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}