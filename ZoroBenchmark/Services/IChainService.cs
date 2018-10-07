using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Zoro.Benchmark.Services
{
    public abstract class IChainService
    {
        protected int Iterations;
        protected int MinThreads;
        protected int MaxThreads;
        protected int MinCompletionPortThreads;
        protected int MaxCompletionPortThreads;

        protected readonly ILogger<IChainService> _logger;
        protected readonly IConfigurationRoot _config;

        public IChainService(ILogger<IChainService> logger, IConfigurationRoot config)
        {
            _logger = logger;
            _config = config;

            Iterations = config.GetSection(this.GetType().ToString()).GetValue<Int32>("Iterations");
            MinThreads = config.GetSection(this.GetType().ToString()).GetValue<Int32>("MinThreads");
            MaxThreads = config.GetSection(this.GetType().ToString()).GetValue<Int32>("MaxThreads");
            MinCompletionPortThreads = config.GetSection(this.GetType().ToString()).GetValue<Int32>("MinCompletionPortThreads");
            MaxCompletionPortThreads = config.GetSection(this.GetType().ToString()).GetValue<Int32>("MaxCompletionPortThreads");
        }

        protected void SetThreadNumbers()
        {

            if (!ThreadPool.SetMinThreads(MinThreads, MinCompletionPortThreads))
            {
                _logger.LogInformation("Can't set SetMinThreads. {0}, {1}",
                   MaxThreads,
                   MinCompletionPortThreads);
                return;
            }

            if (!ThreadPool.SetMaxThreads(MaxThreads, MaxCompletionPortThreads))
            {
                _logger.LogInformation("Can't set MaxThreads. {0}, {1}",
                   MaxThreads,
                   MaxCompletionPortThreads);
                return;
            }
        }

        public abstract void Run();
    }
}
