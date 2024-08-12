using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	internal class WorkviewObject
	{
		public int ObjectId { get; set; }
		public string? ObjectName {  get; set; }
		public string? ClassName { get; set; }
		public string? ApplicationName { get; set; }
		public string? CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? Status { get; set; }
		public int Revision { get; set; }
		public DateTime RevisionDate { get; set; }
		public List<WorkviewAttribute>? Attributes { get; set; }
		public List<WorkviewHistory>? History { get; set; }
	}
}
