using System;

namespace SlimGet.Data
{
    public struct AuthenticationToken
    {
        public string UserId { get; }
        public DateTimeOffset IssuedAt { get; }
        public Guid Guid { get; set; }

        public AuthenticationToken(string userId, DateTimeOffset issuedAt, Guid guid)
        {
            this.UserId = userId;
            this.IssuedAt = issuedAt;
            this.Guid = guid;
        }

        public static AuthenticationToken IssueNew(string userId)
            => new AuthenticationToken(userId, DateTimeOffset.UtcNow, Guid.NewGuid());
    }
}
