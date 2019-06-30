using System.Text;

namespace SlimGet.Services
{
    public class EncodingProvider : IEncodingProvider
    {
        public Encoding TextEncoding => Utilities.UTF8;
    }
}
