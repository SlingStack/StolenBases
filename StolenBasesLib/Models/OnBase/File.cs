using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class File
	{
		public string? FileName {  get; set; }
		public string? FilePath { get; set; }
		public string? MimeType { get; set; }
		public string? Ext { get; set; }
		public int Page { get; set; }
		public int FileSize { get; set; }
		public int ByteRangeStart {  get; set; }
		public int ByteRangeEnd { get; set; }
		public int PageCount {  get; set; }
	}
}
