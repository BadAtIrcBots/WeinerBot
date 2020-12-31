using System;
using System.Globalization;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrumpBot.Services
{
    public class JsonConverters
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> to and from Unix epoch time
        /// </summary>
        public class UnixDateTimeFloatConverter : DateTimeConverterBase
        {
            internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The calling serializer.</param>
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                float seconds;

                if (value is DateTime dateTime)
                {
                    seconds = (float) (dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
                }
                else
                {
                    throw new JsonSerializationException("Expected date object value.");
                }

                if (seconds < 0)
                {
                    throw new JsonSerializationException(
                        "Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
                }

                writer.WriteValue(seconds);
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <returns>The object value.</returns>
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
                JsonSerializer serializer)
            {
                float seconds;

                if (reader.TokenType == JsonToken.Integer)
                {
                    seconds = (float) reader.Value!;
                }
                else if (reader.TokenType == JsonToken.String)
                {
                    if (!float.TryParse((string) reader.Value!, out seconds))
                    {
                        throw new Exception(
                            "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture,
                                objectType));
                    }
                }
                else
                {
                    throw new Exception(
                        "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(
                            CultureInfo.InvariantCulture, reader.TokenType));
                }

                if (seconds >= 0)
                {
                    DateTime d = UnixEpoch.AddSeconds(seconds);
                    return d;
                }

                throw new Exception(
                    "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}."
                        .FormatWith(CultureInfo.InvariantCulture, objectType));
            }
        }
    }
}