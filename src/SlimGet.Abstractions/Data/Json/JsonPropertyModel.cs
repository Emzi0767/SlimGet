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

using System.Reflection;

namespace SlimGet.Data
{
    internal sealed class JsonPropertyModel
    {
        public PropertyInfo Property { get; }
        public string Name { get; }
        public JsonNullHandling NullHandling { get; }

        public JsonPropertyModel(PropertyInfo prop, string name, JsonNullHandling nullHandling)
        {
            this.Property = prop;
            this.Name = name;
            this.NullHandling = nullHandling;
        }
    }
}
