namespace LoadSplitter
{
	public class ThreadPoolConfig
	{
		public int MaxWorkerThreads { get; set; } = 60;
		public int MaxCompletionPorts { get; set; } = 60;
		public int MaxDoneEvents { get; set; } = 60; // CANNOT EXCEED 64

		public int DataChunkSize { get; set; } = 1000;
	}
}