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
using System.Text.Json.Serialization;
using SlimGet.Data;

namespace SlimGet.Attributes
{
    /// <summary>
    /// Overrides global behaviour for null value handling for a given property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class JsonNullHandlingAttribute : JsonAttribute
    {
        /// <summary>
        /// Gets the null handling policy for the property.
        /// </summary>
        public JsonNullHandling NullHandling { get; }

        /// <summary>
        /// Overrides global behaviour for null value handling for a given property.
        /// </summary>
        /// <param name="nullHandling">Specifies how to handle null values on this property.</param>
        public JsonNullHandlingAttribute(JsonNullHandling nullHandling)
        {
            this.NullHandling = nullHandling;
        }
    }
}
