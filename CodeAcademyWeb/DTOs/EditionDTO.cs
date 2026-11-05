using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Course edition (summary).</summary>
	public class EditionDTO
	{
		/// <summary>Edition id.</summary>
		public long Id { get; set; }
		/// <summary>Edition code.</summary>
		public string Code { get; set; }
		/// <summary>Edition description.</summary>
		public string Description { get; set; }
		/// <summary>Actual price of the edition.</summary>
		public decimal RealPrice { get; set; }
	}
}
