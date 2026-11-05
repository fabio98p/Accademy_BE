using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Didactic area.</summary>
	public class AreaDTO
	{
		/// <summary>Area id.</summary>
		public long Id { get; set; }
		/// <summary>Area name.</summary>
		public string Name { get; set; }
		/// <summary>Area description.</summary>
		public string Description { get; set; }
	}
}
