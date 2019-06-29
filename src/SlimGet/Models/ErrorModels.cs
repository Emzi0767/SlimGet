using System;
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public class SimpleErrorModel
    {
        [JsonProperty("request_id", NullValueHandling = NullValueHandling.Include)]
        public string RequestId { get; }

        public SimpleErrorModel(string reqId)
        {
            this.RequestId = reqId;
        }
    }

    public sealed class DeveloperErrorModel : SimpleErrorModel
    {
        [JsonProperty("path", NullValueHandling = NullValueHandling.Include)]
        public string Path { get; }

        [JsonProperty("exception", NullValueHandling = NullValueHandling.Include)]
        public ExceptionModel Exception { get; }

        public DeveloperErrorModel(string reqId, string path, Exception ex)
            : base(reqId)
        {
            this.Path = path;

            if (ex != null)
                this.Exception = new ExceptionModel(ex);
        }
    }

    public sealed class ExceptionModel
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Include)]
        public string Type { get; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Include)]
        public string Message { get; }

        [JsonProperty("stack_trace", NullValueHandling = NullValueHandling.Include)]
        public string StackTrace { get; }

        [JsonProperty("inner_exception", NullValueHandling = NullValueHandling.Include)]
        public ExceptionModel InnerException { get; }

        public ExceptionModel(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex), "Exception cannot be null.");

            this.Type = ex.GetType().ToString();
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
                this.InnerException = new ExceptionModel(ex.InnerException);
        }
    }
}
