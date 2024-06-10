using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class WorkflowTransaction
	{
		public int TransactionNum { get; set; }
		public string? Lifecycle { get; set; }
		public string? Queue { get; set; }
		public string? User { get; set; }
		public DateTime LogDate { get; set; }
		public string? Message { get; set; }
	}
}
