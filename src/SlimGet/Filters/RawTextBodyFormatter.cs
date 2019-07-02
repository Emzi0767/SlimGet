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
