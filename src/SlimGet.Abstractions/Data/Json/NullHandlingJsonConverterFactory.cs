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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlimGet.Data
{
    /// <summary>
    /// Creates instances of <see cref="NullHandlingJsonConverter{T}"/>.
    /// </summary>
    public sealed class NullHandlingJsonConverterFactory : JsonConverterFactory
    {
        private static MethodInfo CreateMeth { get; } = GetMeth();

        private readonly ConcurrentDictionary<Type, JsonConverter> _converters;

        public NullHandlingJsonConverterFactory()
        {
            this._converters = new ConcurrentDictionary<Type, JsonConverter>();
        }

        public override bool CanConvert(Type typeToConvert)
            => JsonTypeModel.UsesCustomSerializer(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => this._converters.GetOrAdd(typeToConvert, (t, o) => CreateGMC(t, o), options);

        private static JsonConverter CreateGMC(Type t, JsonSerializerOptions opts)
            => CreateMeth
                .MakeGenericMethod(t)
                .Invoke(null, new[] { opts })
                as JsonConverter;

        private static JsonConverter<T> Create<T>(JsonSerializerOptions opts)
            where T : class
            => new NullHandlingJsonConverter<T>(opts);

        private static MethodInfo GetMeth()
        {
            var t = typeof(NullHandlingJsonConverterFactory);
            return t.GetMethod(nameof(Create), BindingFlags.NonPublic | BindingFlags.Static)
                .GetGenericMethodDefinition();
        }
    }
}
