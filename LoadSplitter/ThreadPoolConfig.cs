namespace LoadSplitter
{
	public class ThreadPoolConfig
	{
		public int MaxWorkerThreads { get; set; } = 40;
		public int MaxCompletionPorts { get; set; } = 40;

		public int DataChunkSize { get; set; } = 107;
	}
}