using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Enrollment of a student in a course edition.</summary>
	public class EnrollmentDTO
	{
		/// <summary>Enrollment id.</summary>
		public long Id { get; set; }
		/// <summary>Student id.</summary>
		public long StudentId { get; set; }
		/// <summary>Student first name (read only).</summary>
		public string StudentFirstname { get; set; }
		/// <summary>Student last name (read only).</summary>
		public string StudentLastname { get; set; }
		/// <summary>Edition id.</summary>
		public long CourseEditionId { get; set; }
		/// <summary>Edition code (read only).</summary>
		public string CourseEditionCode { get; set; }
		/// <summary>Course title (read only).</summary>
		public string CourseEditionCourseTitle { get; set; }
		//public string EnrollmentDate { get; set; }
		/// <summary>Evaluation given to the student.</summary>
		public string StudentEvaluation { get; set; }
		/// <summary>Feedback given by the student.</summary>
		public string StudentFeedback { get; set; }
		/// <summary>Edition start date (read only).</summary>
		public string CourseEditionStartDate { get; set; }
	}
}
