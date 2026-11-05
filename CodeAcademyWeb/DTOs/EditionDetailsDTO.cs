using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Course edition with course and instructor details.</summary>
	public class EditionDetailsDTO
	{
		/// <summary>Edition id.</summary>
		public long Id { get; set; }
		/// <summary>Edition code.</summary>
		public string Code { get; set; }
		/// <summary>Edition description.</summary>
		public string Description { get; set; }
		/// <summary>Start date.</summary>
		public string StartDate { get; set; } 
		/// <summary>End date.</summary>
		public string FinalizationDate { get; set; } 
		/// <summary>Actual price of the edition.</summary>
		public decimal RealPrice { get; set; }
		/// <summary>Course id.</summary>
		public long CourseId { get; set; }
		/// <summary>Course title (read only).</summary>
		public string CourseTitle { get; set; }
		/// <summary>Instructor id.</summary>
		public long InstructorId { get; set; }
		/// <summary>Instructor full name (read only).</summary>
		public string InstructorFullName { get; set; }
		/// <summary>Instructor first name (read only).</summary>
		public string InstructorFirstname { get; set; }
		/// <summary>Instructor last name (read only).</summary>
		public string InstructorLastname { get; set; }
	}
}
