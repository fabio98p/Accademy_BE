using AcademyModel;
using AcademyModel.Entities;
using AcademyModel.Exceptions;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class CourseControllerTests
	{
		private readonly ICoursesService service = Substitute.For<ICoursesService>();
		private readonly CourseController controller;

		public CourseControllerTests()
		{
			controller = new CourseController(service, MapperFactory.Create());
		}

		private static CourseDTO CourseDTO(long id = 0, string creationDate = "2026-01-10") => new CourseDTO
		{
			Id = id,
			Title = "C# Fundamentals",
			Description = "Basics of C#",
			Duration = 40,
			BasePrice = 500m,
			Syllabus = "Types, classes, LINQ",
			Level = (int)Level.INTERMEDIATE,
			AreaId = 2,
			GrantsCertification = true,
			CreationDate = creationDate
		};

		private static void AssertCourse(CourseDTO dto)
		{
			Assert.Equal(1, dto.Id);
			Assert.Equal("C# Fundamentals", dto.Title);
			Assert.Equal("Basics of C#", dto.Description);
			Assert.Equal(40, dto.Duration);
			Assert.Equal(500m, dto.BasePrice);
			Assert.Equal("Types, classes, LINQ", dto.Syllabus);
			Assert.Equal((int)Level.INTERMEDIATE, dto.Level);
			Assert.Equal(2, dto.AreaId);
			Assert.Equal("Development", dto.AreaName);
			Assert.True(dto.GrantsCertification);
			Assert.Equal("2026-01-10", dto.CreationDate);
		}

		private void ServiceCreatesWithId(long id)
		{
			service.CreateCourse(Arg.Any<Course>()).Returns(call =>
			{
				var course = call.Arg<Course>();
				course.Id = id;
				return course;
			});
		}

		private static void AssertNotFoundErrorObject(IActionResult result)
		{
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			var error = Assert.IsType<ErrorObject>(notFound.Value);
			Assert.Equal(404, error.StatusCode);
		}

		[Fact]
		public void GetAllCourse_WithCourses_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetAllCourses().Returns(new List<Course> { TestData.Course() });

			// Act
			var result = controller.GetAllCourse();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertCourse(Assert.Single(Assert.IsAssignableFrom<IEnumerable<CourseDTO>>(ok.Value)));
		}

		[Fact]
		public void GetAllCourse_NoCourses_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetAllCourses().Returns(new List<Course>());

			// Act
			var result = controller.GetAllCourse();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<CourseDTO>>(ok.Value));
		}

		[Fact]
		public void GetLastNCurses_AnyN_PassesNToServiceAndReturnsMappedDTOs()
		{
			// Arrange
			service.GetLastCourses(3).Returns(new List<Course> { TestData.Course() });

			// Act
			var result = controller.GetLastNCurses(3);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertCourse(Assert.Single(Assert.IsAssignableFrom<IEnumerable<CourseDTO>>(ok.Value)));
			service.Received(1).GetLastCourses(3);
		}

		[Fact]
		public void GetLastNCurses_NoCourses_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetLastCourses(3).Returns(new List<Course>());

			// Act
			var result = controller.GetLastNCurses(3);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<CourseDTO>>(ok.Value));
		}

		[Fact]
		public void GetCourseById_ExistingId_ReturnsOkWithMappedDTO()
		{
			// Arrange
			service.GetCourseById(1).Returns(TestData.Course());

			// Act
			var result = controller.GetCourseById(1);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertCourse(Assert.IsType<CourseDTO>(ok.Value));
		}

		[Fact] // Expected red: returns 200 with null
		public void GetCourseById_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange
			service.GetCourseById(99).Returns((Course)null);

			// Act
			var result = controller.GetCourseById(99);

			// Assert
			AssertNotFoundErrorObject(result);
		}

		[Fact]
		public void CreateCourse_ValidCourse_ReturnsCreatedWithMappedDTO()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var result = controller.CreateCourse(CourseDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			var dto = Assert.IsType<CourseDTO>(created.Value);
			Assert.Equal(1, dto.Id);
			Assert.Equal("C# Fundamentals", dto.Title);
			Assert.Equal(2, dto.AreaId);
			service.Received(1).CreateCourse(Arg.Is<Course>(c => c.Title == "C# Fundamentals" && c.Level == Level.INTERMEDIATE));
		}

		[Fact] // Expected red: Location points to /api/courses/{id}
		public void CreateCourse_ValidCourse_LocationPointsToCourse()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var result = controller.CreateCourse(CourseDTO());

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal("/api/Course/1", created.Location);
		}

		[Fact] // Expected red: the creation date is not set server-side
		public void CreateCourse_WithoutCreationDate_SetsTodayAsCreationDate()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var result = controller.CreateCourse(CourseDTO(creationDate: null));

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal(TestData.Today, Assert.IsType<CourseDTO>(created.Value).CreationDate);
		}

		[Fact] // Expected red: the creation date sent by the client is kept
		public void CreateCourse_WithClientCreationDate_SetsTodayAsCreationDate()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var result = controller.CreateCourse(CourseDTO(creationDate: "2020-05-01"));

			// Assert
			var created = Assert.IsType<CreatedResult>(result);
			Assert.Equal(TestData.Today, Assert.IsType<CourseDTO>(created.Value).CreationDate);
		}

		[Fact] // Expected red: returns 201 Created
		public void UpdateCourse_ValidCourse_ReturnsOkWithMappedDTO()
		{
			// Arrange
			service.UpdateCourse(Arg.Any<Course>()).Returns(call => call.Arg<Course>());

			// Act
			var result = controller.UpdateCourse(CourseDTO(1));

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<CourseDTO>(ok.Value);
			Assert.Equal(1, dto.Id);
			Assert.Equal("C# Fundamentals", dto.Title);
			service.Received(1).UpdateCourse(Arg.Is<Course>(c => c.Id == 1));
		}

		[Fact]
		public void RemoveCourse_WithBody_ReturnsOkWithDTOAndDeletesCourse()
		{
			// Act
			var result = controller.RemoveCourse(CourseDTO(1));

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<CourseDTO>(ok.Value);
			Assert.Equal(1, dto.Id);
			Assert.Equal("C# Fundamentals", dto.Title);
			service.Received(1).DeleteCourse(Arg.Is<Course>(c => c.Id == 1));
		}

		[Fact]
		public void RemoveCourse_ExistingId_ReturnsOkWithDTOAndDeletesCourse()
		{
			// Arrange
			service.GetCourseById(1).Returns(TestData.Course());

			// Act
			var result = controller.RemoveCourse(1L);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			AssertCourse(Assert.IsType<CourseDTO>(ok.Value));
			service.Received(1).DeleteCourse(1L);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public void RemoveCourse_NonExistentId_ReturnsNotFoundWithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			service.GetCourseById(99).Returns((Course)null);
			service.When(s => s.DeleteCourse(99L)).Throw(new ArgumentNullException("entity"));

			// Act
			var result = controller.RemoveCourse(99L);

			// Assert
			AssertNotFoundErrorObject(result);
		}
	}
}
