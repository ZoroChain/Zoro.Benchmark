using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zoro.Benchmark.Services
{
    public class WalletService : IChainService
    {
        private CountdownEvent done;
        private TimeSpan totalLatency = TimeSpan.FromSeconds(0);
        private BlockingCollection<string> traceMessages
            = new BlockingCollection<string>();

        private DateTime startTime;

        public WalletService(ILogger<WalletService> logger, IConfigurationRoot config) : base(logger, config)
        {
            done = new CountdownEvent(Iterations);
        }

        async public override Task Run(Dictionary<String, Object> args)
        {
            _logger.LogInformation("Start Service {0}", this.GetType().ToString());

            base.SetThreadNumbers();

            startTime = DateTime.Now;

            for (int i = 0; i < Iterations; i++)
            {
                var queueTime = DateTime.Now;
                int id = i;
                ThreadPool.QueueUserWorkItem(async (o) =>
                {
                    OnTaskStart(id, queueTime);
                    // add a task here to simulate neo operation
                    //await Task.Delay(500);
                    Task.Run(() => {
                        _logger.LogDebug("Thread: {1}, Is pool thread: {2}",
                           Thread.CurrentThread.GetHashCode(),
                           Thread.CurrentThread.IsThreadPoolThread);
                        Task.Delay(100);
                    }).Wait(1000);
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }

            done.Wait();

            foreach (var message in traceMessages)
            {
                _logger.LogInformation(message);
            }

            _logger.LogInformation("Duration = {0} ms, Average latency = {1}", totalLatency.TotalMilliseconds,
                 TimeSpan.FromMilliseconds(totalLatency.TotalMilliseconds / Iterations));

            _logger.LogInformation("Ending Service {0}", this.GetType().ToString());
        }

        private void OnTaskStart(int id, DateTime queueTime)
        {
            var latency = DateTime.Now - queueTime;
            lock (done) totalLatency += latency;
            Log(id, queueTime, "Starting");
        }

        private void OnTaskEnd(int id, DateTime queueTime)
        {
            Log(id, queueTime, "Finished");
            done.Signal();
        }

        private void Log(int id, DateTime queueTime, string action)
        {
            var now = DateTime.Now;
            var timestamp = now - startTime;
            var latency = now - queueTime;
            var msg = string.Format("{0}: {1} {2,3}, latency = {3}", timestamp, action, id, latency);
            traceMessages.Add(msg);
        }
    }
}
