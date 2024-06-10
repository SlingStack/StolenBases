using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class Rendition
	{
		public bool IsDefault {  get; set; }
		public int RenditionId {  get; set; }
		public string? CreatedBy { get; set; }
		public DateTime CreatedDate {  get; set; }
		public string? Comment {  get; set; }
		public List<File>? Files { get; set; }
	}
}
