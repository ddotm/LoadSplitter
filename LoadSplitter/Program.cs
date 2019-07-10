using System;
using System.Collections.Generic;
using System.Threading;

namespace LoadSplitter
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			// Fetch data to be processed
			var dataContainer = new LoadContainer();
			var config = new ThreadPoolConfig();
			ProcessData(config, dataContainer.DataItems);

			Console.ReadKey();
		}

		private static void ProcessData(ThreadPoolConfig config, List<DataItem> data)
		{
			ThreadPool.GetMaxThreads(out var workers, out var ports);
			Console.WriteLine($"Max worker threads: {workers}. Max completion port threads: {ports}");

			ThreadPool.SetMaxThreads(config.MaxWorkerThreads, config.MaxCompletionPorts);

			var numItemsToProcess = data.Count;
			var startIndex = 0;
			while (numItemsToProcess > 0)
			{
				var nextChunkSize = numItemsToProcess >= config.DataChunkSize ? config.DataChunkSize : numItemsToProcess;
				var dataToProcess = data.GetRange(startIndex, nextChunkSize);

				numItemsToProcess -= nextChunkSize;
				startIndex += nextChunkSize;

				ThreadPool.QueueUserWorkItem(ThreadFunc, dataToProcess);
			}
		}

		private static void ThreadFunc(object data)
		{
			var thread = Thread.CurrentThread;
			var dataToProcess = (List<DataItem>) data;

			// Process the data
			foreach (var dataItem in dataToProcess)
			{
				Console.WriteLine($"Item: {dataItem.Id} {dataItem.Value} (Thread Id: {thread.ManagedThreadId})");
			}
		}
	}
}