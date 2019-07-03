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

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace SlimGet.Filters
{
    public sealed class RawTextBodyFormatter : InputFormatter
    {
        public RawTextBodyFormatter()
        {
            this.SupportedMediaTypes.Add("text/plain");
        }

        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                return false;

            if (context.HttpContext.Request.ContentType == "text/plain")
                return true;

            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            using (var sr = new StreamReader(context.HttpContext.Request.Body))
                return await InputFormatterResult.SuccessAsync(await sr.ReadToEndAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
