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
