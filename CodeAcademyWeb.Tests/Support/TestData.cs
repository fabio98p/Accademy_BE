using AcademyModel;
using AcademyModel.Entities;
using NodaTime;
using System;
using System.Text.Json;
using Xunit;

namespace CodeAcademyWeb.Tests.Support
{
	/// <summary>Sample entities shared by unit and HTTP tests.</summary>
	public static class TestData
	{
		/// <summary>
		/// Asserts that <paramref name="actual"/> (yyyy-MM-dd) is today's date. <paramref name="dayBeforeAct"/> is
		/// <see cref="DateTime.Today"/> read before the call: both days are accepted if the test runs across midnight.
		/// </summary>
		public static void AssertIsToday(DateTime dayBeforeAct, string actual)
		{
			var accepted = new[] { dayBeforeAct.ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd") };
			Assert.Contains(actual, accepted);
		}

		public static Area Area() => new Area { Id = 2, Name = "Development", Description = "Software development" };

		public static Course Course(long id = 1) => new Course
		{
			Id = id,
			Title = "C# Fundamentals",
			Description = "Basics of C#",
			Duration = 40,
			BasePrice = 500m,
			Syllabus = "Types, classes, LINQ",
			Level = Level.INTERMEDIATE,
			AreaId = 2,
			Area = Area(),
			GrantsCertification = true,
			CreationDate = new LocalDate(2026, 1, 10)
		};

		public static Instructor Instructor() => new Instructor
		{
			Id = 3,
			Firstname = "Mario",
			Lastname = "Rossi",
			DateOfBirth = new LocalDate(1980, 3, 15)
		};

		public static Edition Edition(long id = 5) => new Edition
		{
			Id = id,
			Code = "CS-2026-01",
			Description = "Spring edition",
			StartDate = new LocalDate(2026, 3, 2),
			FinalizationDate = new LocalDate(2026, 4, 30),
			RealPrice = 450m,
			CourseId = 1,
			Course = Course(),
			InstructorId = 3,
			Instructor = Instructor()
		};

		public static Student Student(long id = 4) => new Student
		{
			Id = id,
			Firstname = "Laura",
			Lastname = "Bianchi",
			DateOfBirth = new LocalDate(1995, 7, 20),
			Address = "Via Verdi 2",
			City = "Torino",
			Email = "laura.bianchi@example.com",
			PhoneNumber = "3209876543",
			IsEmployee = true
		};

		public static Enrollment Enrollment(long id = 10) => new Enrollment
		{
			Id = id,
			StudentId = 4,
			Student = Student(),
			CourseEditionId = 5,
			CourseEdition = Edition(),
			EnrollmentDate = new LocalDate(2026, 2, 1),
			StudentEvaluation = "Good",
			StudentFeedback = "Great course"
		};

		/// <summary>Parses the JSON body of an HTTP response.</summary>
		public static JsonElement ParseJson(string body)
		{
			using var document = JsonDocument.Parse(body);
			return document.RootElement.Clone();
		}
	}
}
