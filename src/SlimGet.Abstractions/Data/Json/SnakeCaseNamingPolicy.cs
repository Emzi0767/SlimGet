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
using System.Text;
using System.Text.Json;

namespace SlimGet.Data
{
    internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private const string Separator = "_";

        public override string ConvertName(string name)
        {
            // https://github.com/J0rgeSerran0/JsonNamingPolicy/blob/master/JsonSnakeCaseNamingPolicy.cs

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) return string.Empty;

            ReadOnlySpan<char> spanName = name.Trim();

            var stringBuilder = new StringBuilder();
            var addCharacter = true;

            var isPreviousSpace = false;
            var isPreviousSeparator = false;
            var isCurrentSpace = false;
            var isNextLower = false;
            var isNextUpper = false;
            var isNextSpace = false;

            for (var position = 0; position < spanName.Length; position++)
            {
                if (position != 0)
                {
                    isCurrentSpace = spanName[position] == 32;
                    isPreviousSpace = spanName[position - 1] == 32;
                    isPreviousSeparator = spanName[position - 1] == 95;

                    if (position + 1 != spanName.Length)
                    {
                        isNextLower = spanName[position + 1] > 96 && spanName[position + 1] < 123;
                        isNextUpper = spanName[position + 1] > 64 && spanName[position + 1] < 91;
                        isNextSpace = spanName[position + 1] == 32;
                    }

                    if (isCurrentSpace &&
                        (isPreviousSpace ||
                        isPreviousSeparator ||
                        isNextUpper ||
                        isNextSpace))
                        addCharacter = false;
                    else
                    {
                        var isCurrentUpper = spanName[position] > 64 && spanName[position] < 91;
                        var isPreviousLower = spanName[position - 1] > 96 && spanName[position - 1] < 123;
                        var isPreviousNumber = spanName[position - 1] > 47 && spanName[position - 1] < 58;

                        if (isCurrentUpper &&
                        (isPreviousLower ||
                        isPreviousNumber ||
                        isNextLower ||
                        isNextSpace ||
                        (isNextLower && !isPreviousSpace)))
                            stringBuilder.Append(Separator);
                        else
                        {
                            if (isCurrentSpace &&
                                !isPreviousSpace &&
                                !isNextSpace)
                            {
                                stringBuilder.Append(Separator);
                                addCharacter = false;
                            }
                        }
                    }
                }

                if (addCharacter)
                    stringBuilder.Append(spanName[position]);
                else
                    addCharacter = true;
            }

            return stringBuilder.ToString().ToLower();
        }
    }
}
