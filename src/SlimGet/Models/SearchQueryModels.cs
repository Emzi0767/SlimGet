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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Models
{
    public sealed class SearchQueryModel
    {
        [FromQuery(Name = "q"), StringLength(32767, ErrorMessage = "Package ID query is too long.")]
        public string Query { get; set; }

        [FromQuery(Name = "skip"), Range(0, int.MaxValue, ErrorMessage = "Skip amount cannot be less than 0.")]
        public int Skip { get; set; }

        [FromQuery(Name = "take"), Range(1, 1000, ErrorMessage = "You must specify at least 1, and at most 1000 items per page.")]
        public int Take { get; set; }

        [FromQuery(Name = "prerelease")]
        public bool Prerelase { get; set; }

        [FromQuery(Name = "semVerLevel")]
        public string SemVerLevel { get; set; }
    }

    public sealed class SearchEnumerateModel
    {
        [FromQuery(Name = "id"), StringLength(32767, ErrorMessage = "Package ID is too long.")]
        public string Id { get; set; }

        [FromQuery(Name = "prerelease")]
        public bool Prerelase { get; set; }

        [FromQuery(Name = "semVerLevel")]
        public string SemVerLevel { get; set; }
    }
}
