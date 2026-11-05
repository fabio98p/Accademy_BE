using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Instructor.</summary>
	public class InstructorDTO
	{
		/// <summary>Instructor id.</summary>
		public long Id { get; set; }
		/// <summary>First name.</summary>
		public string Firstname { get; set; }
		/// <summary>Last name.</summary>
		public string Lastname { get; set; }
		/// <summary>Date of birth.</summary>
		public string DateOfBirth { get; set; }
		/// <summary>Address.</summary>
		public string Address { get; set; }
		/// <summary>City.</summary>
		public string City { get; set; }
		/// <summary>Email.</summary>
		public string Email { get; set; }
		/// <summary>Phone number.</summary>
		public string PhoneNumber { get; set; }
		/// <summary>Whether the instructor is a contractor.</summary>
		public bool IsContractor { get; set; }
		/// <summary>Pay rate.</summary>
		public decimal PayRate { get; set; }
	}
}
