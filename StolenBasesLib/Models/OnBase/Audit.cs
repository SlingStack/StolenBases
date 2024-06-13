using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class Audit
	{
		public List<TransactionLog> TransactionLog { get; set; } = new List<TransactionLog>();
		public List<WorkflowLog> WorkflowLog { get; set; } = new List<WorkflowLog>();
		public List<WorkflowTransaction> WorkflowTransactions { get; set; } = new List<WorkflowTransaction>();
	}
}
