namespace SlimGet.Data.Configuration
{
    public sealed class RedisConfiguration
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public int Index { get; set; }
        public string Password { get; set; }
    }
}
