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
using SlimGet.Data.Database;

namespace SlimGet.Data
{
    public struct SymbolIdentifier : IEquatable<SymbolIdentifier>
    {
        public Guid Identifier { get; }
        public int Age { get; }
        public SymbolKind Kind { get; }

        public SymbolIdentifier(Guid id, int age, SymbolKind kind)
        {
            this.Identifier = id;
            this.Age = age;
            this.Kind = kind;
        }

        public override bool Equals(object obj)
            => obj is SymbolIdentifier other ? this == other : false;

        public bool Equals(SymbolIdentifier other)
            => this == other;

        public override int GetHashCode()
        {
            var hash = 13;

            hash = hash * 7 + this.Identifier.GetHashCode();
            hash = hash * 7 + this.Age.GetHashCode();
            hash = hash * 7 + this.Kind.GetHashCode();

            return hash;
        }

        public static bool operator ==(SymbolIdentifier left, SymbolIdentifier right)
            => left.Identifier == right.Identifier && left.Age == right.Age && left.Kind == right.Kind;

        public static bool operator !=(SymbolIdentifier left, SymbolIdentifier right)
            => left.Identifier != right.Identifier || left.Age != right.Age || left.Kind != right.Kind;
    }
}
