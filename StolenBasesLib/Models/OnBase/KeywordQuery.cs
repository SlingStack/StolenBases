using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	[Table("OBKeywordQueries")]
	public class KeywordQuery : BaseModel
	{
		[Column("doctype")]
		public string? DocType { get; set; }

		[Column("query")]
		public string? Query { get; set; }

	}
}
