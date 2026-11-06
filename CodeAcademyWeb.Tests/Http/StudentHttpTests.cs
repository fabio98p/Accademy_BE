using AcademyModel.BuisnessLogic;
using AcademyModel.Entities;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using NodaTime;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CodeAcademyWeb.Tests.Http
{
	[Trait("Category", "Http")]
	public class StudentHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public StudentHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
		}

		private static StudentDTO StudentDTO(long id = 0) => new StudentDTO
		{
			Id = id,
			Firstname = "Laura",
			Surname = "Bianchi",
			DateOfBirth = "1995-07-20",
			Address = "Via Verdi 2",
			City = "Torino",
			Email = "laura.bianchi@example.com",
			PhoneNumber = "3209876543",
			IsEmployee = true
		};

		private async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
			TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

		private static void AssertStudentJson(JsonElement student)
		{
			Assert.Equal(4, student.GetProperty("id").GetInt64());
			Assert.Equal("Laura", student.GetProperty("firstname").GetString());
			Assert.Equal("Bianchi", student.GetProperty("surname").GetString());
			Assert.Equal("1995-07-20", student.GetProperty("dateOfBirth").GetString());
			Assert.Equal("laura.bianchi@example.com", student.GetProperty("email").GetString());
			Assert.True(student.GetProperty("isEmployee").GetBoolean());
		}

		private async Task AssertNotFoundErrorObject(HttpResponseMessage response)
		{
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Equal(404, (await ReadJson(response)).GetProperty("statusCode").GetInt32());
		}

		[Fact]
		public async Task GetAll_WithStudents_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.PeopleService.GetAllStudents().Returns(new List<Student> { TestData.Student() });

			// Act
			var response = await client.GetAsync("/api/Student", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertStudentJson(Assert.Single((await ReadJson(response)).EnumerateArray()));
		}

		[Fact]
		public async Task GetAll_NoStudents_Returns200WithEmptyArray()
		{
			// Arrange
			factory.PeopleService.GetAllStudents().Returns(new List<Student>());

			// Act
			var response = await client.GetAsync("/api/Student", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}

		[Fact]
		public async Task Create_ValidStudent_Returns201WithLocationAndBody()
		{
			// Arrange
			factory.PeopleService.CreateStudent(Arg.Do<Student>(s => s.Id = 4));

			// Act
			var response = await client.PostAsJsonAsync("/api/Student", StudentDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal("/api/student/4", response.Headers.Location?.OriginalString);
			AssertStudentJson(await ReadJson(response));
			factory.PeopleService.Received(1).CreateStudent(Arg.Is<Student>(s =>
				s.Lastname == "Bianchi" &&
				s.DateOfBirth == new LocalDate(1995, 7, 20)));
		}

		[Fact] // Expected red: returns 201 Created
		public async Task UpdateStudent_ValidStudent_Returns200WithBody()
		{
			// Act
			var response = await client.PutAsJsonAsync("/api/Student", StudentDTO(4), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertStudentJson(await ReadJson(response));
			factory.PeopleService.Received(1).UpdateStudent(Arg.Is<Student>(s => s.Id == 4));
		}

		[Fact]
		public async Task GetStudentById_ExistingId_Returns200WithStudent()
		{
			// Arrange
			factory.PeopleService.GetStudentById(4).Returns(TestData.Student());

			// Act
			var response = await client.GetAsync("/api/Student/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertStudentJson(await ReadJson(response));
		}

		[Fact] // Expected red: returns 204 No Content
		public async Task GetStudentById_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange
			factory.PeopleService.GetStudentById(99).Returns((Student)null);

			// Act
			var response = await client.GetAsync("/api/Student/99", TestContext.Current.CancellationToken);

			// Assert
			await AssertNotFoundErrorObject(response);
		}

		[Fact]
		public async Task EnrollStudent_IdMismatch_Returns400WithErrorObject()
		{
			// Act
			var response = await client.PostAsJsonAsync("/api/Student/4/enrollments", new EnrollDataDTO { IdStudent = 5, IdEdition = 5 }, TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Equal(400, (await ReadJson(response)).GetProperty("statusCode").GetInt32());
			factory.EnrollmentsService.DidNotReceive().EnrollSudentToEdition(Arg.Any<EnrollData>());
		}

		[Fact] // Expected red: the enrollments service is never assigned (NullReferenceException)
		public async Task EnrollStudent_ValidData_Returns201WithEnrollment()
		{
			// Arrange
			factory.EnrollmentsService.EnrollSudentToEdition(Arg.Any<EnrollData>()).Returns(TestData.Enrollment());

			// Act
			var response = await client.PostAsJsonAsync("/api/Student/4/enrollments", new EnrollDataDTO { IdStudent = 4, IdEdition = 5 }, TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal("/api/student/4/enrollments/10", response.Headers.Location?.OriginalString);
			var json = await ReadJson(response);
			Assert.Equal(10, json.GetProperty("id").GetInt64());
			Assert.Equal(4, json.GetProperty("studentId").GetInt64());
			Assert.Equal(5, json.GetProperty("courseEditionId").GetInt64());
			factory.EnrollmentsService.Received(1).EnrollSudentToEdition(Arg.Is<EnrollData>(d => d.IdStudent == 4 && d.IdEdition == 5));
		}

		[Fact] // Expected red: the student is read after the deletion
		public async Task DeleteStudent_ExistingId_Returns200WithStudentReadBeforeDeletion()
		{
			// Arrange: like the real service, the student is no longer found after the deletion
			var deleted = false;
			factory.PeopleService.GetStudentById(4).Returns(_ => deleted ? null : TestData.Student());
			factory.PeopleService.When(s => s.DeleteStudent(4L)).Do(_ => deleted = true);

			// Act
			var response = await client.DeleteAsync("/api/Student/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertStudentJson(await ReadJson(response));
			factory.PeopleService.Received(1).DeleteStudent(4L);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public async Task DeleteStudent_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			factory.PeopleService.GetStudentById(99).Returns((Student)null);
			factory.PeopleService.When(s => s.DeleteStudent(99L)).Throw(new ArgumentNullException("entity"));

			// Act
			var response = await client.DeleteAsync("/api/Student/99", TestContext.Current.CancellationToken);

			// Assert
			await AssertNotFoundErrorObject(response);
		}
	}
}
