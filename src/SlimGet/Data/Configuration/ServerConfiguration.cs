namespace SlimGet.Data.Configuration
{
    public class ServerConfiguration
    {
        public CertificateConfiguration SslCertificate { get; set; }
        public long MaxRequestSizeBytes { get; set; }
    }
}
