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
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SlimGet.Filters
{
    [HtmlTargetElement("opengraph",
        Attributes = "og-title,og-site-name",
        TagStructure = TagStructure.WithoutEndTag)]
    public sealed class OpenGraphTagHelper : TagHelper
    {
        [HtmlAttributeName("og-title")]
        public string Title { get; set; }

        [HtmlAttributeName("og-description")]
        public string Description { get; set; }

        [HtmlAttributeName("og-type")]
        public string Type { get; set; } = "website";

        [HtmlAttributeName("og-image")]
        public Uri Image { get; set; }

        [HtmlAttributeName("og-url")]
        public Uri Url { get; set; }

        [HtmlAttributeName("og-colour-theme")]
        public string ColourTheme { get; set; }

        [HtmlAttributeName("og-site-name")]
        public string SiteName { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(this.Title))
                sb.AppendLine($"<meta name=\"og:title\" content=\"{WebUtility.HtmlEncode(this.Title)}\" />");
            if (!string.IsNullOrWhiteSpace(this.Description))
                sb.AppendLine($"<meta name=\"og:description\" content=\"{WebUtility.HtmlEncode(this.Description)}\" />");
            if (!string.IsNullOrWhiteSpace(this.Type))
                sb.AppendLine($"<meta name=\"og:type\" content=\"{this.Type}\" />");
            if (this.Image != null)
                sb.AppendLine($"<meta name=\"og:image\" content=\"{WebUtility.HtmlEncode(this.Image.ToString())}\" />");
            if (this.Url != null)
                sb.AppendLine($"<meta name=\"og:url\" content=\"{WebUtility.HtmlEncode(this.Url.ToString())}\" />");
            if (!string.IsNullOrWhiteSpace(this.SiteName))
                sb.AppendLine($"<meta name=\"og:site_name\" content=\"{this.SiteName}\" />");
            if (!string.IsNullOrWhiteSpace(this.ColourTheme))
                sb.AppendLine($"<meta name=\"theme-color\" content=\"{this.ColourTheme}\" />");

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
