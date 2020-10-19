// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Emzi0767.Utilities;
using SlimGet.Attributes;

namespace SlimGet.Data
{
    /// <summary>
    /// JSON converter with special handling for null values via <see cref="JsonNullHandlingAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Type of object this converter handles.</typeparam>
    public sealed class NullHandlingJsonConverter<T> : JsonConverter<T>
        where T : class
    {
        private JsonTypeModel Model { get; }

        public NullHandlingJsonConverter(JsonSerializerOptions opts)
        {
            this.Model = JsonTypeModel.Get<T>(opts.PropertyNamingPolicy);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected object start, got '{reader.TokenType}' instead.");

            var obj = ReflectionUtilities.CreateEmpty<T>();
            var model = this.Model;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"Expected property name, got '{reader.TokenType}' instead.");

                var name = reader.GetString();
                if (!reader.Read())
                    throw new EndOfStreamException("Reached end of JSON stream.");

                var prop = model.GetJsonProperty(name);
                if (prop == null)
                    throw new JsonException($"Property '{name}' is not present on object of type '{typeToConvert}'.");

                var val = JsonSerializer.Deserialize(ref reader, prop.Property.PropertyType, options);
                prop.Property.SetValue(obj, val);
            }

            return obj;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var model = this.Model;
            foreach (var (_, prop) in model.Properties)
            {
                var writeNull = prop.NullHandling == JsonNullHandling.Include
                    || (prop.NullHandling == JsonNullHandling.Default && !options.IgnoreNullValues);

                var val = prop.Property.GetValue(value);
                if (val == null && !writeNull)
                    continue;

                writer.WritePropertyName(prop.Name);
                if (val == null)
                    writer.WriteNullValue();
                else
                    JsonSerializer.Serialize(writer, val, prop.Property.PropertyType, options);
            }

            writer.WriteEndObject();
        }
    }
}
