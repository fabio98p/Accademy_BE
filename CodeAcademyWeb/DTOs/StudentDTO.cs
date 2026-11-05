using AcademyModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.DTOs
{
	/// <summary>Student.</summary>
	public class StudentDTO
	{
		/// <summary>Student id.</summary>
		public long Id { get; set; }
		/// <summary>First name.</summary>
		public string Firstname { get; set; }
		/// <summary>Last name.</summary>
		public string Surname { get; set; }
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
		/// <summary>Whether the student is an employee.</summary>
		public bool IsEmployee { get; set; }
		
		public StudentDTO(){ }
		public StudentDTO(Student original) 
		{
			Id = original.Id;
			Firstname = original.Firstname;
			Surname = original.Lastname;
			Address = original.Address;
			City = original.City;
			Email = original.Email;
			PhoneNumber = original.PhoneNumber;
			IsEmployee = original.IsEmployee;
			DateOfBirth = original.DateOfBirth.ToString("yyyy/MM/dd", null);
		}
	}
}
