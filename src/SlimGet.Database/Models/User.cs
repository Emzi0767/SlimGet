using System;
using System.Collections.Generic;

namespace SlimGet.Data.Database
{
    public sealed class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<Token> Tokens { get; set; }
        public List<Package> Packages { get; set; }
    }
}
