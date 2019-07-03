using System;

namespace SlimGet.Data
{
    public struct PdbMetadata
    {
        public Guid Identifier { get; }
        public int Age { get; }

        public PdbMetadata(Guid id, int age)
        {
            this.Identifier = id;
            this.Age = age;
        }
    }
}
