using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class WorkflowLog
	{
		public string? Lifecycle { get; set; }
		public string? Queue { get; set; }
		public string? EntryUser { get; set; }
		public DateTime EntryDate { get; set; }
		public DateTime ExitDate { get; set; }
		public string? ExitUser { get; set; }
		public string? ToQueue { get; set; }
	}
}
