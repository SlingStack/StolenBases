using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class Audit
	{
		public List<TransactionLog>? TransactionLog { get; set; }
		public List<WorkflowLog>? WorkflowLog { get; set; }
		public List<WorkflowTransaction>? WorkflowTransactions { get; set; }
	}
}
