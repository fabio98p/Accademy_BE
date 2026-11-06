using AcademyModel.Entities;
using AcademyModel.Services;
using CodeAcademyWeb.Controllers;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CodeAcademyWeb.Tests.Controllers
{
	[Trait("Category", "Unit")]
	public class InstructorControllerTests
	{
		private readonly IPeopleService service = Substitute.For<IPeopleService>();
		private readonly InstructorController controller;

		public InstructorControllerTests()
		{
			controller = new InstructorController(service, MapperFactory.Create());
		}

		[Fact]
		public void GetAllInstructor_WithInstructors_ReturnsOkWithMappedDTOs()
		{
			// Arrange
			service.GetInstructors().Returns(new List<Instructor>
			{
				new Instructor
				{
					Id = 1,
					Firstname = "Mario",
					Lastname = "Rossi",
					DateOfBirth = new LocalDate(1980, 3, 15),
					Address = "Via Roma 1",
					City = "Milano",
					Email = "mario.rossi@example.com",
					PhoneNumber = "3331234567",
					IsContractor = true,
					PayRate = 45.5m
				}
			});

			// Act
			var result = controller.GetAllInstructor();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.Single(Assert.IsAssignableFrom<IEnumerable<InstructorDTO>>(ok.Value));
			Assert.Equal(1, dto.Id);
			Assert.Equal("Mario", dto.Firstname);
			Assert.Equal("Rossi", dto.Lastname);
			Assert.Equal("Via Roma 1", dto.Address);
			Assert.Equal("Milano", dto.City);
			Assert.Equal("mario.rossi@example.com", dto.Email);
			Assert.Equal("3331234567", dto.PhoneNumber);
			Assert.True(dto.IsContractor);
			Assert.Equal(45.5m, dto.PayRate);
		}

		[Fact]
		public void GetAllInstructor_NoInstructors_ReturnsOkWithEmptyList()
		{
			// Arrange
			service.GetInstructors().Returns(new List<Instructor>());

			// Act
			var result = controller.GetAllInstructor();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Empty(Assert.IsAssignableFrom<IEnumerable<InstructorDTO>>(ok.Value));
		}
	}
}
