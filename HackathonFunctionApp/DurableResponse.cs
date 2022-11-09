using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonFunctionApp
{
	public class DurableResponse
	{
		public string Id { get; set; }
		public string StatusQueryGetUri { get; set; }
		public string SendEventPostUri { get; set; }
		public string TerminatePostUri { get; set; }
		public string PurgeHistoryDeleteUri { get; set; }
		public string RestartPostUri { get; set; }
	}
}
