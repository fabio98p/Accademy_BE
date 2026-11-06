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
	public class InstructorHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public InstructorHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
		}

		[Fact]
		public async Task GetAllInstructor_WithInstructors_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.PeopleService.GetInstructors().Returns(new List<Instructor>
			{
				new Instructor
				{
					Id = 1,
					Firstname = "Mario",
					Lastname = "Rossi",
					DateOfBirth = new LocalDate(1980, 3, 15),
					Email = "mario.rossi@example.com",
					IsContractor = true,
					PayRate = 45.5m
				}
			});

			// Act
			var response = await client.GetAsync("/api/Instructor", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			var instructor = Assert.Single(json.RootElement.EnumerateArray());
			Assert.Equal(1, instructor.GetProperty("id").GetInt64());
			Assert.Equal("Mario", instructor.GetProperty("firstname").GetString());
			Assert.Equal("Rossi", instructor.GetProperty("lastname").GetString());
			Assert.Equal("mario.rossi@example.com", instructor.GetProperty("email").GetString());
			Assert.True(instructor.GetProperty("isContractor").GetBoolean());
			Assert.Equal(45.5m, instructor.GetProperty("payRate").GetDecimal());
		}

		[Fact]
		public async Task GetAllInstructor_NoInstructors_Returns200WithEmptyArray()
		{
			// Arrange
			factory.PeopleService.GetInstructors().Returns(new List<Instructor>());

			// Act
			var response = await client.GetAsync("/api/Instructor", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var instructors = await response.Content.ReadFromJsonAsync<List<InstructorDTO>>(ApiFactory.Json, TestContext.Current.CancellationToken);
			Assert.Empty(instructors);
		}
	}
}
