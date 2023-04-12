
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Dungeoner.Importers;

public class Rect2ListConverter : JsonConverter<List<Rect2>> {
    public override List<Rect2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        List<Rect2> rects = new();
        while(reader.Read() && reader.TokenType == JsonTokenType.StartArray) {
            int x, y, width, height;
            if(!reader.Read() || reader.TokenType != JsonTokenType.Number) {
                throw new JsonException("Expecting number for x position of Rect");
            } else {
                x = reader.GetInt32();
            }
            if(!reader.Read() || reader.TokenType != JsonTokenType.Number) {
                throw new JsonException("Expecting number for y position of Rect");
            } else {
                y = reader.GetInt32();
            }
            if(!reader.Read() || reader.TokenType != JsonTokenType.Number) {
                throw new JsonException("Expecting number for width of Rect");
            } else {
                width = reader.GetInt32();
            }
            if(!reader.Read() || reader.TokenType != JsonTokenType.Number) {
                throw new JsonException("Expecting number for height of Rect");
            } else {
                height = reader.GetInt32();
            }

            if(!reader.Read() || reader.TokenType != JsonTokenType.EndArray) {
                throw new JsonException("Expecting only 4 values for Rect (x, y, width, height)");
            }
            rects.Add(new(x, y, width, height));
        }
        return rects;
    }

    public override void Write(Utf8JsonWriter writer, List<Rect2> value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}