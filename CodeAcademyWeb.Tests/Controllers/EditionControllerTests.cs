using AcademyModel.Entities;
using AcademyModel.Exceptions;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.Generic;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class EditionControllerTests
	{
		private readonly IEditionsService service = Substitute.For<IEditionsService>();
		private readonly EditionController controller;

		public EditionControllerTests()
		{
			controller = new EditionController(service, MapperFactory.Create());
		}

		private static EditionDetailsDTO EditionDetailsDTO(long id = 0) => new EditionDetailsDTO
		{
			Id = id,
			Code = "CS-2026-01",
			Description = "Spring edition",
			StartDate = "2026-03-02",
			FinalizationDate = "2026-04-30",
			RealPrice = 450m,
			CourseId = 1,
			InstructorId = 3
		};

		private static void AssertEditionDetails(EditionDetailsDTO dto)
		{
			Assert.Equal(5, dto.Id);
			Assert.Equal("CS-2026-01", dto.Code);
			Assert.Equal("Spring edition", dto.Description);
			Assert.Equal("2026-03-02", dto.StartDate);
			Assert.Equal("2026-04-30", dto.FinalizationDate);
			Assert.Equal(450m, dto.RealPrice);
			Assert.Equal(1, dto.CourseId);
			Assert.Equal("C# Fundamentals", dto.CourseTitle);
			Assert.Equal(3, dto.InstructorId);
			Assert.Equal("Mario Rossi", dto.InstructorFullName);
			Assert.Equal("Mario", dto.InstructorFirstname);
			Assert.Equal("Rossi", dto.InstructorLastname);
		}

		[Fact]
		public void GetAll_WithEditions_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetAllEditions().Returns(new List<Edition> { TestData.Edition() });

			// Act
			var result = controller.GetAll();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.Single(Assert.IsAssignableFrom<IEnumerable<EditionDTO>>(ok.Value));
			Assert.Equal(5, dto.Id);
			Assert.Equal("CS-2026-01", dto.Code);
			Assert.Equal("Spring edition", dto.Description);
			Assert.Equal(450m, dto.RealPrice);
		}

		[Fact]
		public void GetAll_NoEditions_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetAllEditions().Returns(new List<Edition>());

			// Act
			var result = controller.GetAll();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<EditionDTO>>(ok.Value));
		}

		[Fact]
		public void FindById_ExistingId_ReturnsOkWithEditionDetails()
		{
			// Arrange
			service.GetEditionById(5).Returns(TestData.Edition());

			// Act
			var result = controller.FindById(5);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertEditionDetails(Assert.IsType<EditionDetailsDTO>(ok.Value));
		}

		[Fact]
		public void FindById_NonExistentId_ReturnsNotFound()
		{
			// Arrange
			service.GetEditionById(99).Returns((Edition)null);

			// Act
			var result = controller.FindById(99);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public void Create_ValidEdition_ReturnsCreatedWithLocation()
		{
			// Arrange
			service.When(s => s.CreateCourseEdition(Arg.Any<Edition>())).Do(call => call.Arg<Edition>().Id = 5);

			// Act
			var result = controller.Create(EditionDetailsDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal("/api/edition/5", created.Location);
			var dto = Assert.IsType<EditionDetailsDTO>(created.Value);
			Assert.Equal(5, dto.Id);
			Assert.Equal("2026-03-02", dto.StartDate);
			Assert.Equal("2026-04-30", dto.FinalizationDate);
			service.Received(1).CreateCourseEdition(Arg.Is<Edition>(e =>
				e.Code == "CS-2026-01" &&
				e.StartDate == new LocalDate(2026, 3, 2) &&
				e.FinalizationDate == new LocalDate(2026, 4, 30) &&
				e.CourseId == 1 &&
				e.InstructorId == 3));
		}

		[Fact]
		public void Create_RelatedEntityNotFound_ReturnsBadRequestWithErrorObject()
		{
			// Arrange
			service.CreateCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException("Course not found", nameof(Course)));

			// Act
			var result = controller.Create(EditionDetailsDTO());

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(badRequest.Value);
			Assert.Equal(400, error.StatusCode);
			Assert.Equal("Course not found", error.Message);
		}

		[Fact]
		public void Edit_ValidEdition_ReturnsOkWithMappedDTO()
		{
			// Act
			var result = controller.Edit(EditionDetailsDTO(5));

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<EditionDetailsDTO>(ok.Value);
			Assert.Equal(5, dto.Id);
			Assert.Equal("CS-2026-01", dto.Code);
			Assert.Equal("2026-03-02", dto.StartDate);
			service.Received(1).EditCourseEdition(Arg.Is<Edition>(e => e.Id == 5));
		}

		[Theory]
		[InlineData(nameof(Course))]
		[InlineData(nameof(Instructor))]
		public void Edit_RelatedEntityNotFound_ReturnsBadRequestWithErrorObject(string entityName)
		{
			// Arrange
			service.EditCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException($"{entityName} not found", entityName));

			// Act
			var result = controller.Edit(EditionDetailsDTO(5));

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(badRequest.Value);
			Assert.Equal(400, error.StatusCode);
			Assert.Equal($"{entityName} not found", error.Message);
		}

		[Fact] // Expected red: the 404 body is a plain string
		public void Edit_EditionNotFound_ReturnsNotFoundWithErrorObject()
		{
			// Arrange
			service.EditCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException("Edition not found", nameof(Edition)));

			// Act
			var result = controller.Edit(EditionDetailsDTO(99));

			// Assert
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(notFound.Value);
			Assert.Equal(404, error.StatusCode);
			Assert.Equal("Edition not found", error.Message);
		}

		[Fact]
		public void Delete_ExistingId_ReturnsNoContent()
		{
			// Act
			var result = controller.Delete(5);

			// Assert
			Assert.IsType<NoContentResult>(result);
			service.Received(1).DeleteCourseEdition(5);
		}

		[Fact]
		public void Delete_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange
			service.When(s => s.DeleteCourseEdition(99)).Throw(new EntityNotFoundException("Edition not found", nameof(Edition)));

			// Act
			var result = controller.Delete(99);

			// Assert
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(notFound.Value);
			Assert.Equal(404, error.StatusCode);
			Assert.Equal("Edition not found", error.Message);
		}

		[Fact]
		public void GetEditionsByCourseId_WithEditions_ReturnsOkWithEditionDetails()
		{
			// Arrange
			service.GetEditionsByCourseId(1).Returns(new List<Edition> { TestData.Edition() });

			// Act
			var result = controller.GetEditionsByCourseId(1);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertEditionDetails(Assert.Single(Assert.IsAssignableFrom<IEnumerable<EditionDetailsDTO>>(ok.Value)));
			service.Received(1).GetEditionsByCourseId(1);
		}

		[Fact]
		public void GetEditionsByCourseId_NoEditions_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetEditionsByCourseId(1).Returns(new List<Edition>());

			// Act
			var result = controller.GetEditionsByCourseId(1);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<EditionDetailsDTO>>(ok.Value));
		}

		[Fact]
		public void GetAvailableEnrollmentByStudentId_WithEditions_ReturnsOkWithEditionDetails()
		{
			// Arrange
			service.GetAvailableEnrollmentByStudentId(4).Returns(new List<Edition> { TestData.Edition() });

			// Act
			var result = controller.GetAvailableEnrollmentByStudentId(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertEditionDetails(Assert.Single(Assert.IsAssignableFrom<IEnumerable<EditionDetailsDTO>>(ok.Value)));
			service.Received(1).GetAvailableEnrollmentByStudentId(4);
		}

		[Fact]
		public void GetAvailableEnrollmentByStudentId_NoEditions_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetAvailableEnrollmentByStudentId(4).Returns(new List<Edition>());

			// Act
			var result = controller.GetAvailableEnrollmentByStudentId(4);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<EditionDetailsDTO>>(ok.Value));
		}
	}
}
