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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SlimGet.Attributes;

namespace SlimGet.Data
{
    internal sealed class JsonTypeModel
    {
        private static readonly ConcurrentDictionary<Type, JsonTypeModel> _typeModels = new ConcurrentDictionary<Type, JsonTypeModel>();

        public Type Type { get; }
        public bool IsCustomSerializable { get; }
        public IReadOnlyDictionary<string, JsonPropertyModel> Properties { get; }
        public IReadOnlyDictionary<string, JsonPropertyModel> JsonProperties { get; }

        private JsonTypeModel(Type type, bool customSerializable, IEnumerable<JsonPropertyModel> props)
        {
            this.Type = type;
            this.IsCustomSerializable = customSerializable;
            this.Properties = props?.ToDictionary(x => x.Property.Name);
            this.JsonProperties = props?.ToDictionary(x => x.Name);
        }

        public JsonPropertyModel GetProperty(string name)
            => this.Properties.TryGetValue(name, out var prop)
                ? prop
                : null;

        public JsonPropertyModel GetJsonProperty(string name)
            => this.JsonProperties.TryGetValue(name, out var prop)
                ? prop
                : null;

        public static bool UsesCustomSerializer(Type t)
            => _typeModels.ContainsKey(t) || t.GetCustomAttribute<JsonOverrideConverterAttribute>() != null;

        public static JsonTypeModel Get<T>(JsonNamingPolicy namingPolicy)
            where T : class
            => _typeModels.GetOrAdd(typeof(T), (_, n) => Build<T>(n), namingPolicy);

        public static JsonTypeModel Get(Type t, JsonNamingPolicy namingPolicy)
            => _typeModels.GetOrAdd(t, (tt, n) => Build(tt, n), namingPolicy);

        private static JsonTypeModel Build<T>(JsonNamingPolicy namingPolicy)
            where T : class
            => Build(typeof(T), namingPolicy);

        private static JsonTypeModel Build(Type t, JsonNamingPolicy namingPolicy)
        {
            var useCustom = t.GetCustomAttribute<JsonOverrideConverterAttribute>() != null;
            if (!useCustom)
                return new JsonTypeModel(t, useCustom, null);

            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propModels = new List<JsonPropertyModel>(props.Length);
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes<JsonAttribute>();
                if (attrs.Any(x => x is JsonIgnoreAttribute))
                    continue;

                var nullHandling = attrs.OfType<JsonNullHandlingAttribute>()
                    .FirstOrDefault()
                    ?.NullHandling
                    ?? JsonNullHandling.Default;

                var name = namingPolicy.ConvertName(prop.Name);

                propModels.Add(new JsonPropertyModel(prop, name, nullHandling));
            }

            return new JsonTypeModel(t, useCustom, propModels);
        }
    }
}
