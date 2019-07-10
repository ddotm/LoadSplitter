namespace LoadSplitter
{
	public class ThreadPoolConfig
	{
		public int MaxWorkerThreads { get; set; } = 10;
		public int MaxCompletionPorts { get; set; } = 10;

		public int DataChunkSize { get; set; } = 100;
	}
}