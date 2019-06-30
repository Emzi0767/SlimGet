using System.Text;

namespace SlimGet.Services
{
    public interface IEncodingProvider
    {
        Encoding TextEncoding { get; }
    }
}
