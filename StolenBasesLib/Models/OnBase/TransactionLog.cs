using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class TransactionLog
	{
		public int TransactionNum { get; set; }
		public string? Message { get; set; }
		public DateTime LogDate { get; set; }
		public string? User { get; set; }
		public int DocumentRevisionNum { get; set; }
	}
}
