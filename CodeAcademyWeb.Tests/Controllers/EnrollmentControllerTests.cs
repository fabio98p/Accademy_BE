using AcademyModel.Entities;
using AcademyModel.Exceptions;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class EnrollmentControllerTests
	{
		private readonly IEnrollmentsService service = Substitute.For<IEnrollmentsService>();
		private readonly EnrollmentController controller;

		public EnrollmentControllerTests()
		{
			controller = new EnrollmentController(service, MapperFactory.Create());
		}

		private static EnrollmentDTO NewEnrollmentDTO() => new EnrollmentDTO
		{
			StudentId = 4,
			CourseEditionId = 5,
			StudentEvaluation = "Good",
			StudentFeedback = "Great course"
		};

		private void ServiceAssignsId(long id)
		{
			service.CreateEnrollment(Arg.Any<Enrollment>()).Returns(call =>
			{
				var enrollment = call.Arg<Enrollment>();
				enrollment.Id = id;
				return enrollment;
			});
		}

		[Fact]
		public void GetEnrollmentByStudentId_WithEnrollments_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetSubscribedEnrollmentByStudentId(4).Returns(new List<Enrollment> { TestData.Enrollment() });

			// Act
			var result = controller.GetEnrollmentByStudentId(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.Single(Assert.IsAssignableFrom<IEnumerable<EnrollmentDTO>>(ok.Value));
			Assert.Equal(10, dto.Id);
			Assert.Equal(4, dto.StudentId);
			Assert.Equal("Laura", dto.StudentFirstname);
			Assert.Equal("Bianchi", dto.StudentLastname);
			Assert.Equal(5, dto.CourseEditionId);
			Assert.Equal("CS-2026-01", dto.CourseEditionCode);
			Assert.Equal("C# Fundamentals", dto.CourseEditionCourseTitle);
			Assert.Equal("2026-03-02", dto.CourseEditionStartDate);
			Assert.Equal("Good", dto.StudentEvaluation);
			Assert.Equal("Great course", dto.StudentFeedback);
		}

		[Fact]
		public void GetEnrollmentByStudentId_AnyId_PassesIdToService()
		{
			// Arrange
			service.GetSubscribedEnrollmentByStudentId(Arg.Any<long>()).Returns(new List<Enrollment>());

			// Act
			controller.GetEnrollmentByStudentId(42);

			// Assert
			service.Received(1).GetSubscribedEnrollmentByStudentId(42);
		}

		[Fact]
		public void GetEnrollmentByStudentId_NoEnrollments_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetSubscribedEnrollmentByStudentId(4).Returns(new List<Enrollment>());

			// Act
			var result = controller.GetEnrollmentByStudentId(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<EnrollmentDTO>>(ok.Value));
		}

		[Fact]
		public void Create_ValidEnrollment_ReturnsCreatedWithMappedDTO()
		{
			// Arrange
			ServiceAssignsId(10);

			// Act
			var result = controller.Create(NewEnrollmentDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			var dto = Assert.IsType<EnrollmentDTO>(created.Value);
			Assert.Equal(10, dto.Id);
			Assert.Equal(4, dto.StudentId);
			Assert.Equal(5, dto.CourseEditionId);
			service.Received(1).CreateEnrollment(Arg.Is<Enrollment>(e => e.StudentId == 4 && e.CourseEditionId == 5));
		}

		[Fact] // Expected red: Location points to /api/edition/{id}
		public void Create_ValidEnrollment_LocationPointsToEnrollment()
		{
			// Arrange
			ServiceAssignsId(10);

			// Act
			var result = controller.Create(NewEnrollmentDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal("/api/Enrollment/10", created.Location);
		}

		[Fact]
		public void Create_RelatedEntityNotFound_ReturnsBadRequestWithErrorObject()
		{
			// Arrange
			service.CreateEnrollment(Arg.Any<Enrollment>()).Throws(new EntityNotFoundException("Student not found", nameof(Student)));

			// Act
			var result = controller.Create(NewEnrollmentDTO());

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(badRequest.Value);
			Assert.Equal(400, error.StatusCode);
			Assert.Equal("Student not found", error.Message);
		}

		[Fact]
		public void DeleteEnroll_ExistingId_ReturnsOkWithoutBody()
		{
			// Act
			var result = controller.DeleteEnroll(10);

			// Assert
			Assert.IsType<OkResult>(result);
			service.Received(1).DeleteEnrollment(10);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public void DeleteEnroll_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			service.When(s => s.DeleteEnrollment(99)).Throw(new ArgumentNullException("entity"));

			// Act
			var result = controller.DeleteEnroll(99);

			// Assert
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(notFound.Value);
			Assert.Equal(404, error.StatusCode);
		}
	}
}
