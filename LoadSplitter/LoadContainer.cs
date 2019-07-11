using System.Collections.Generic;
using System.IO;

namespace LoadSplitter
{
	public class LoadContainer
	{
		public static int NumOfItems { get; set; } = 100000;
		public List<DataItem> DataItems { get; set; }

		public LoadContainer()
		{
			DataItems = PopulateDataItems();
		}

		private static List<DataItem> PopulateDataItems()
		{
			var id = 1;
			var data = new List<DataItem>();
			for (var i = 0; i < NumOfItems; i++)
			{
				data.Add(new DataItem
				{
					Id = id++,
					Value = Path.GetRandomFileName().Replace(".", "")
				});
			}

			return data;
		}
	}
}