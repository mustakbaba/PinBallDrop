using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Color.black;

            try
            {
                string colorHex = reader.Value.ToString();
                ColorUtility.TryParseHtmlString("#" + colorHex, out Color color);
                return color;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("COLOR_CONVERTER", $"Failed to parse color: {ex.Message}");
                return Color.black;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color color = (Color)value;
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            writer.WriteValue(colorHex);
        }
    }
}