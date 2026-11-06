using AcademyModel.Entities;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class LessonControllerTests
	{
		private readonly ILessonsService service = Substitute.For<ILessonsService>();
		private readonly LessonController controller;

		public LessonControllerTests()
		{
			controller = new LessonController(service, MapperFactory.Create());
		}

		[Fact]
		public void FindLessonForEditionId_WithLessons_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.FindLessonForEditionId(7).Returns(new List<Lesson>
			{
				new Lesson { Id = 1, Subject = "Introduction", CourseEditionId = 7, ClassroomId = 3 },
				new Lesson { Id = 2, Subject = "Variables", CourseEditionId = 7, ClassroomId = 4 }
			});

			// Act
			var result = controller.FindLessonForEditionId(7);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dtos = Assert.IsAssignableFrom<IEnumerable<LessonDTO>>(ok.Value).ToList();
			Assert.Equal(2, dtos.Count);
			Assert.Equal(1, dtos[0].Id);
			Assert.Equal("Introduction", dtos[0].Subject);
			Assert.Equal(7, dtos[0].CourseEditionId);
			Assert.Equal(3, dtos[0].ClassroomId);
			Assert.Equal("Variables", dtos[1].Subject);
		}

		[Fact]
		public void FindLessonForEditionId_AnyId_PassesIdToService()
		{
			// Arrange
			service.FindLessonForEditionId(Arg.Any<long>()).Returns(new List<Lesson>());

			// Act
			controller.FindLessonForEditionId(42);

			// Assert
			service.Received(1).FindLessonForEditionId(42);
		}

		[Fact]
		public void FindLessonForEditionId_NoLessons_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.FindLessonForEditionId(7).Returns(new List<Lesson>());

			// Act
			var result = controller.FindLessonForEditionId(7);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<LessonDTO>>(ok.Value));
		}
	}
}
