using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Data to enroll a student in an edition.</summary>
	public class EnrollDataDTO
	{
		/// <summary>Student id.</summary>
		public long IdStudent { get; set; }
		/// <summary>Edition id.</summary>
		public long IdEdition { get; set; }
	}
}
