using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	internal class WorkviewHistory
	{
		public string? AttributeName { get; set; }
		public DateTime transactionDate { get; set; }
		public string? StartValue { get; set; }
		public string? EndValue { get; set; }
		public string? ModifiedBy { get; set; }
	}
}
