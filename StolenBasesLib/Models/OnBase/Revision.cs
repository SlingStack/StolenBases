using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Models.OnBase
{
	public class Revision
	{
		public int RevisionNumber {  get; set; }
		public DateTime RevisionDate { get; set; }
		public string? Comment { get; set; }
		public List<Rendition> Renditions { get; set; }
	}
}
