using System.Collections.Generic;
using System.Threading;

namespace LoadSplitter
{
	public class ThreadRequest
	{
		public List<DataItem> DataItems { get; set; }
		public ManualResetEvent DoneEvent { get; set; }
	}
}