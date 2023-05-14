
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers.JsonConverters;

public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expecting HTML hexidecimal color code.");
        }
        try
        {
            var color = new Color(reader.GetString());
            return color;
        }
        catch (ArgumentOutOfRangeException aore)
        {
            throw new JsonException("Expecting HTML hexidecimal color code", aore);
        }
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}