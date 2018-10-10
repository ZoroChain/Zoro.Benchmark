using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zoro.Benchmark.Data;
using Zoro.Benchmark.Services.Transactions;
using static System.Collections.Generic.Dictionary<string, string>;

namespace Zoro.Benchmark.Services
{
    public class TransactionService : IChainService
    {
        private CountdownEvent done;
        private TimeSpan totalLatency = TimeSpan.FromSeconds(0);
        private BlockingCollection<string> traceMessages 
            = new BlockingCollection<string>();

        private DateTime startTime;

        private Keypairs keypairs;

        public TransactionService(ILogger<IChainService> logger, IConfigurationRoot config) : base(logger, config)
        {
            keypairs = new Keypairs(config, DATA_FROM.FILE);
            Iterations = keypairs.Count;
            done = new CountdownEvent(Iterations);
        }

        /// <summary>
        /// Run the benchmark test by calling async method
        /// @Todo Need to implement the logic between OnTaskStart and OnTaskEnd
        /// </summary>
        async public override Task Run(Dictionary<string, object> args)
        {
            _logger.LogInformation("Start Service {0}", this.GetType().ToString());

            base.SetThreadNumbers();

            startTime = DateTime.Now;

            int i = 0;
            foreach (KeyValuePair<string, string> entry in keypairs)
            {
                ThreadPool.GetAvailableThreads(out int workerThreads, out int portThreads);
                _logger.LogDebug("Current Available Threads. workerThreads:{0}, portThreads:{1}",
                   MaxThreads,
                   MaxCompletionPortThreads);

                var queueTime = DateTime.Now;
                int id = i++;
                ThreadPool.QueueUserWorkItem(async (o) =>
                {
                    OnTaskStart(id, queueTime);
                    // add a task here to simulate neo operation
                    //await Task.Delay(500);
                    //_logger.LogInformation("{0} Thread ID: {1}",
                    //    this.GetType().ToString(), System.Threading.Thread.CurrentThread.ManagedThreadId);

                    _logger.LogDebug("Thread: {1}, Is pool thread: {2}",
                       Thread.CurrentThread.GetHashCode(),
                       Thread.CurrentThread.IsThreadPoolThread);

                    Dictionary<string, object> transferArgs = new Dictionary<string, object>();
                    // transferArgs.Add("wif", entry.Key); // need to change to list if read from file
                    // @TODO
                    transferArgs.Add("wif", "L3yQVZKu7u1etBWbeqfNeX1d19mRJdjZTZxiFt72AhLvxBDJwmne");
                    transferArgs.Add("targetAddress", entry.Value);
                    transferArgs.Add("sendCount", 1);

                    // run transfer async
                    await new TransferOneNeo(_logger, _config).Run(transferArgs);

                    OnTaskEnd(id, queueTime);
                });
            }

            done.Wait();

            foreach (var message in traceMessages)
            {
                _logger.LogDebug(message);
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
