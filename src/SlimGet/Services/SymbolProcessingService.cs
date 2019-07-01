using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SlimGet.Services
{
    public sealed class SymbolProcessingService
    {
        private ILogger<SymbolProcessingService> Logger { get; }

        public SymbolProcessingService(ILoggerFactory logger)
        {
            this.Logger = logger.CreateLogger<SymbolProcessingService>();
        }
    }
}
