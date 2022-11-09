using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonFunctionApp
{
	public class DurableStatusResponse
	{
		public string Name { get; set; }
		public string InstanceId { get; set; }
		public string RuntimeStatus { get; set; }
		public object Input { get; set; }
		public object CustomStatus { get; set; }
		public object Output { get; set; }
		public DateTime CreatedTime { get; set; }
		public DateTime LastUpdatedTime { get; set; }
	}
}
