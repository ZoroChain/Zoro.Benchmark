# How to start the benchmark test?
  Just run the program. Program start from `Main` method.

# Adding services
  There are many services that NEO is supplying such as Transaction, Wallet, etc.
  For each scenario, you need to create a `YourService` class which inherites the IChainService abstract class.
  After adding 1 line of code in the ConfigureNeoServices method you can start your test.
  `serviceCollection.AddTransient<IChainService, YourService>();`

# Change Max Threads and Logging
  Please change the settings in appsetttings.json
```
	{
	  "Zoro.Benchmark.Services.TransactionService": {
		"MinThreads": 1,
		"MinCompletionPortThreads": 1,
		"MaxThreads": 1,
		"MaxCompletionPortThreads": 1,
		"Iterations": 1000
	  },
	  "Zoro.Benchmark.Services.WalletService": {
		"MinThreads": 1,
		"MinCompletionPortThreads": 1,
		"MaxThreads": 1,
		"MaxCompletionPortThreads": 1,
		"Iterations": 1000
	  },
	  "Logging": {
		"LogLevel": {
		  "Default": "Debug",
		  "System": "Information",
		  "Microsoft": "Information"
		},
		"Console": {
		  "IncludeScopes": true
		}
	  }
	}
```