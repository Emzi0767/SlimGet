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
