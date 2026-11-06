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
	public class AreaControllerTests
	{
		private readonly IAreasService service = Substitute.For<IAreasService>();
		private readonly AreaController controller;

		public AreaControllerTests()
		{
			controller = new AreaController(service, MapperFactory.Create());
		}

		[Fact]
		public void GetAllAreas_WithAreas_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetAllAreas().Returns(new List<Area>
			{
				new Area { Id = 1, Name = "Development", Description = "Software development" },
				new Area { Id = 2, Name = "Design", Description = "UI and UX" }
			});

			// Act
			var result = controller.GetAllAreas();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dtos = Assert.IsAssignableFrom<IEnumerable<AreaDTO>>(ok.Value).ToList();
			Assert.Equal(2, dtos.Count);
			Assert.Equal(1, dtos[0].Id);
			Assert.Equal("Development", dtos[0].Name);
			Assert.Equal("Software development", dtos[0].Description);
			Assert.Equal(2, dtos[1].Id);
			Assert.Equal("Design", dtos[1].Name);
		}

		[Fact]
		public void GetAllAreas_NoAreas_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetAllAreas().Returns(new List<Area>());

			// Act
			var result = controller.GetAllAreas();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<AreaDTO>>(ok.Value));
		}
	}
}
