using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class Document
	{
		public int DocumentHandle { get; set; }
		public string? DocumentName { get; set; }
		public string? DocTypeGroup {  get; set; }
		public string? DocType {  get; set; }
		public DateTime DocumentDate { get; set; }
		public DateTime StorageDate { get; set; }
		public string? CreatedBy { get; set; }
		public string? Status { get; set; }
		public int Batchnum { get; set; }
		public List<Keyword>? Keywords { get; set; }
		public List<Revision>? Revisions { get; set; }
		public Audit? Audit { get; set; }
	}
}
