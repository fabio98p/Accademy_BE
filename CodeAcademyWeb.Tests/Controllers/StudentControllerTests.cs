using AcademyModel.Entities;
using AcademyModel.Exceptions;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class StudentControllerTests
	{
		private readonly IPeopleService service = Substitute.For<IPeopleService>();
		private readonly StudentController controller;

		public StudentControllerTests()
		{
			// Built with the current constructor: IEnrollmentsService is never injected
			controller = new StudentController(service, MapperFactory.Create());
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

		private static void AssertStudent(StudentDTO dto)
		{
			Assert.Equal(4, dto.Id);
			Assert.Equal("Laura", dto.Firstname);
			Assert.Equal("Bianchi", dto.Surname);
			Assert.Equal("1995-07-20", dto.DateOfBirth);
			Assert.Equal("Via Verdi 2", dto.Address);
			Assert.Equal("Torino", dto.City);
			Assert.Equal("laura.bianchi@example.com", dto.Email);
			Assert.Equal("3209876543", dto.PhoneNumber);
			Assert.True(dto.IsEmployee);
		}

		private static void AssertNotFoundErrorObject(IActionResult result)
		{
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(notFound.Value);
			Assert.Equal(404, error.StatusCode);
		}

		[Fact]
		public void GetAll_WithStudents_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetAllStudents().Returns(new List<Student> { TestData.Student() });

			// Act
			var result = controller.GetAll();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertStudent(Assert.Single(Assert.IsAssignableFrom<IEnumerable<StudentDTO>>(ok.Value)));
		}

		[Fact]
		public void GetAll_NoStudents_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetAllStudents().Returns(new List<Student>());

			// Act
			var result = controller.GetAll();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<StudentDTO>>(ok.Value));
		}

		[Fact]
		public void Create_ValidStudent_ReturnsCreatedWithLocationAndMappedDTO()
		{
			// Arrange
			service.CreateStudent(Arg.Do<Student>(s => s.Id = 4));

			// Act
			var result = controller.Create(StudentDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal("/api/student/4", created.Location);
			AssertStudent(Assert.IsType<StudentDTO>(created.Value));
			service.Received(1).CreateStudent(Arg.Is<Student>(s =>
				s.Lastname == "Bianchi" &&
				s.DateOfBirth == new LocalDate(1995, 7, 20)));
		}

		[Fact] // Expected red: returns 201 Created
		public void UpdateStudent_ValidStudent_ReturnsOkWithMappedDTO()
		{
			// Act
			var result = controller.UpdateStudent(StudentDTO(4));

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertStudent(Assert.IsType<StudentDTO>(ok.Value));
			service.Received(1).UpdateStudent(Arg.Is<Student>(s => s.Id == 4 && s.Lastname == "Bianchi"));
		}

		[Fact]
		public void GetStudentById_ExistingId_ReturnsOkWithMappedDTO()
		{
			// Arrange
			service.GetStudentById(4).Returns(TestData.Student());

			// Act
			var result = controller.GetStudentById(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertStudent(Assert.IsType<StudentDTO>(ok.Value));
		}

		[Fact] // Expected red: returns 200 with null
		public void GetStudentById_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange
			service.GetStudentById(99).Returns((Student)null);

			// Act
			var result = controller.GetStudentById(99);

			// Assert
			AssertNotFoundErrorObject(result);
		}

		[Fact]
		public void EnrollStudent_IdMismatch_ReturnsBadRequestWithErrorObject()
		{
			// Act
			var result = controller.EnrollStudent(new EnrollDataDTO { IdStudent = 5, IdEdition = 5 }, 4);

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(badRequest.Value);
			Assert.Equal(400, error.StatusCode);
		}

		[Fact] // Expected red: the enrollments service is never assigned (NullReferenceException)
		public void EnrollStudent_ValidData_ReturnsCreatedWithEnrollment()
		{
			// Act
			var result = controller.EnrollStudent(new EnrollDataDTO { IdStudent = 4, IdEdition = 5 }, 4);

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal("/api/student/4/enrollments/10", created.Location);
			var dto = Assert.IsType<EnrollmentDTO>(created.Value);
			Assert.Equal(10, dto.Id);
			Assert.Equal(4, dto.StudentId);
			Assert.Equal(5, dto.CourseEditionId);
		}

		[Fact] // Expected red: the student is read after the deletion
		public void DeleteStudent_ExistingId_ReturnsOkWithStudentReadBeforeDeletion()
		{
			// Arrange: like the real service, the student is no longer found after the deletion
			var deleted = false;
			service.GetStudentById(4).Returns(_ => deleted ? null : TestData.Student());
			service.When(s => s.DeleteStudent(4L)).Do(_ => deleted = true);

			// Act
			var result = controller.DeleteStudent(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertStudent(Assert.IsType<StudentDTO>(ok.Value));
			service.Received(1).DeleteStudent(4L);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public void DeleteStudent_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			service.GetStudentById(99).Returns((Student)null);
			service.When(s => s.DeleteStudent(99L)).Throw(new ArgumentNullException("entity"));

			// Act
			var result = controller.DeleteStudent(99);

			// Assert
			AssertNotFoundErrorObject(result);
		}
	}
}
