using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models
{
	[Table("ConversionItems")]
	public class ConversionItem : BaseModel
	{
		[PrimaryKey("id")]
		public int ConversionId { get; set; }

		[Column("source")]
		public string? Source { get; set; }

		[Column("source_name")]
		public string? SourceName { get; set; }

		[Column("source_id")]
		public int SourceId { get; set; }

		[Column("source_type")]
		public string? SourceType { get; set; }

		[Column("converted")]
		public bool WasConverted {  get; set; }

		[Column("data")]
		public string? Data { get; set; }
	}
}
