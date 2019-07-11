using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LoadSplitter
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var stopwatch = Stopwatch.StartNew();
			stopwatch.Start();

			var config = new ThreadPoolConfig();

			// Fetch data to be processed
			var dataContainer = new LoadContainer();
			// Process data with multiple threads
			ProcessDataWithThreadPool(config, dataContainer.DataItems);

			stopwatch.Stop();
			var multithreadElapsed = stopwatch.Elapsed;
			
			// SINGLE THREAD PROCESSING for benchmarking
//			stopwatch.Reset();
//			stopwatch.Start();
//			ProcessDataItems(dataContainer.DataItems, 0);
//			stopwatch.Stop();
//			Console.WriteLine(
//				$"1 thread took {stopwatch.Elapsed} to process {dataContainer.DataItems.Count}");

			Console.WriteLine(
				$"{config.MaxWorkerThreads} threads took {multithreadElapsed} to process {dataContainer.DataItems.Count}");


			Console.ReadKey();
		}

		private static void ProcessDataWithThreadPool(ThreadPoolConfig config, List<DataItem> data)
		{
			var doneEvents = new List<ManualResetEvent>();
			var reqs = new List<ThreadRequest>();
			var unprocessedCount = 0;

			ThreadPool.GetMaxThreads(out var workers, out var ports);
			Console.WriteLine($"Max worker threads: {workers}. Max completion port threads: {ports}");

			ThreadPool.SetMaxThreads(config.MaxWorkerThreads, config.MaxCompletionPorts);

			var numItemsToProcess = data.Count;
			var startIndex = 0;
			while (numItemsToProcess > 0)
			{
				if (doneEvents.Count > config.MaxDoneEvents)
				{
					WaitHandle.WaitAll(doneEvents.ToArray());
					doneEvents.RemoveAll(x =>
					{
						x.Close();
						return x.SafeWaitHandle.IsClosed == true;
					});
					unprocessedCount += reqs.Count(x => x.ProcessingDone == false && x.DoneEvent.SafeWaitHandle.IsClosed == true);
					reqs.RemoveAll(x => x.DoneEvent.SafeWaitHandle.IsClosed == true);
					continue;
				}

				var nextChunkSize =
					numItemsToProcess >= config.DataChunkSize ? config.DataChunkSize : numItemsToProcess;
				var dataToProcess = data.GetRange(startIndex, nextChunkSize);
				
				Console.WriteLine(
					$"Processing next {nextChunkSize} items of {numItemsToProcess} starting at {startIndex}");

				numItemsToProcess -= nextChunkSize;
				startIndex += nextChunkSize;

				var doneEvent = new ManualResetEvent(false);
				doneEvents.Add(doneEvent);

				var threadRequest = new ThreadRequest
				{
					DataItems = dataToProcess,
					DoneEvent = doneEvent,
					ProcessingDone = false
				};
				reqs.Add(threadRequest);

				ThreadPool.QueueUserWorkItem(ThreadFunc, threadRequest);
			}

			WaitHandle.WaitAll(doneEvents.ToArray());
			unprocessedCount += reqs.Count(x => x.ProcessingDone == false);
			Console.WriteLine($"All threads done processing. Unprocessed item count {unprocessedCount}");
		}

		private static void ThreadFunc(object request)
		{
			var thread = Thread.CurrentThread;
			var threadRequest = (ThreadRequest) request;
			var dataToProcess = threadRequest.DataItems;
			var doneEvent = threadRequest.DoneEvent;

			// Process the data
			ProcessDataItems(dataToProcess, thread.ManagedThreadId);
			threadRequest.ProcessingDone = true;
			doneEvent.Set();
		}

		private static void ProcessDataItems(List<DataItem> dataToProcess, int threadId)
		{
			foreach (var dataItem in dataToProcess)
			{
				// Thread.Sleep(50);
				Console.WriteLine($"Item: {dataItem.Id} {dataItem.Value} (Thread Id: {threadId})");
			}
		}
	}
}