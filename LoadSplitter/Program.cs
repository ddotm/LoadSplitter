using System;
using System.Collections.Generic;
using System.Linq;
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
			var doneEvents = new List<ManualResetEvent>();

			ThreadPool.GetMaxThreads(out var workers, out var ports);
			Console.WriteLine($"Max worker threads: {workers}. Max completion port threads: {ports}");

			ThreadPool.SetMaxThreads(config.MaxWorkerThreads, config.MaxCompletionPorts);

			var numItemsToProcess = data.Count;
			var startIndex = 0;
			while (numItemsToProcess > 0)
			{
				var nextChunkSize = numItemsToProcess >= config.DataChunkSize ? config.DataChunkSize : numItemsToProcess;
				var dataToProcess = data.GetRange(startIndex, nextChunkSize);

				Console.WriteLine($"Processing next {nextChunkSize} items of {numItemsToProcess} starting at {startIndex}");

				numItemsToProcess -= nextChunkSize;
				startIndex += nextChunkSize;

				var doneEvent = new ManualResetEvent(false);
				doneEvents.Add(doneEvent);
				var threadRequest = new ThreadRequest
				{
					DataItems = dataToProcess,
					DoneEvent = doneEvent
				};
				ThreadPool.QueueUserWorkItem(ThreadFunc, threadRequest);
			}

			WaitHandle.WaitAll(doneEvents.ToArray());
			var unprocessedItems = data.Where(x => x.Done == false).ToList();
			Console.WriteLine($"All threads done processing. Unprocessed item count {unprocessedItems.Count}");
		}

		private static void ThreadFunc(object request)
		{
			var thread = Thread.CurrentThread;
			var threadRequest = (ThreadRequest) request;
			var dataToProcess = threadRequest.DataItems;
			var doneEvent = threadRequest.DoneEvent;

			// Process the data
			foreach (var dataItem in dataToProcess)
			{
				dataItem.Done = true;
				Console.WriteLine($"Item: {dataItem.Id} {dataItem.Value} (Thread Id: {thread.ManagedThreadId})");
			}

			doneEvent.Set();
		}
	}
}