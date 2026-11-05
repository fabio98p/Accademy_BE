using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Course.</summary>
	public class CourseDTO
	{
		/// <summary>Course id.</summary>
		public long Id { get; set; }
		/// <summary>Course title.</summary>
		public string Title { get; set; }
		/// <summary>Course description.</summary>
		public string Description { get; set; }
		/// <summary>Duration in hours.</summary>
		public int Duration { get; set; }
		/// <summary>Base price.</summary>
		public decimal BasePrice { get; set; }
		/// <summary>Course syllabus.</summary>
		public string Syllabus { get; set; }
		/// <summary>Course level.</summary>
		public int Level { get; set; }
		/// <summary>Id of the area the course belongs to.</summary>
		public long AreaId { get; set; }
		/// <summary>Name of the area (read only).</summary>
		public string AreaName { get; set; }
		/// <summary>Whether the course grants a certification.</summary>
		public bool GrantsCertification { get; set; }
		/// <summary>Creation date.</summary>
		public string CreationDate { get; set; }
	}
}
