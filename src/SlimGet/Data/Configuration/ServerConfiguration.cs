namespace SlimGet.Data.Configuration
{
    public class ServerConfiguration : ITokenConfiguration
    {
        public CertificateConfiguration SslCertificate { get; set; }
        public long MaxRequestSizeBytes { get; set; }
        public string TokenHmacKey { get; set; }
    }
}
