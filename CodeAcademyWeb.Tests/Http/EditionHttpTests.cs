using AcademyModel.Entities;
using AcademyModel.Exceptions;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
	public class EditionHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public EditionHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
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

		private async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
			TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

		private static void AssertEditionDetailsJson(JsonElement edition)
		{
			Assert.Equal(5, edition.GetProperty("id").GetInt64());
			Assert.Equal("CS-2026-01", edition.GetProperty("code").GetString());
			Assert.Equal("2026-03-02", edition.GetProperty("startDate").GetString());
			Assert.Equal("2026-04-30", edition.GetProperty("finalizationDate").GetString());
			Assert.Equal("C# Fundamentals", edition.GetProperty("courseTitle").GetString());
			Assert.Equal("Mario Rossi", edition.GetProperty("instructorFullName").GetString());
		}

		[Fact]
		public async Task GetAll_WithEditions_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.EditionsService.GetAllEditions().Returns(new List<Edition> { TestData.Edition() });

			// Act
			var response = await client.GetAsync("/api/Edition", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var edition = Assert.Single((await ReadJson(response)).EnumerateArray());
			Assert.Equal(5, edition.GetProperty("id").GetInt64());
			Assert.Equal("CS-2026-01", edition.GetProperty("code").GetString());
			Assert.Equal("Spring edition", edition.GetProperty("description").GetString());
			Assert.Equal(450m, edition.GetProperty("realPrice").GetDecimal());
		}

		[Fact]
		public async Task GetAll_NoEditions_Returns200WithEmptyArray()
		{
			// Arrange
			factory.EditionsService.GetAllEditions().Returns(new List<Edition>());

			// Act
			var response = await client.GetAsync("/api/Edition", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}

		[Fact]
		public async Task FindById_ExistingId_Returns200WithEditionDetails()
		{
			// Arrange
			factory.EditionsService.GetEditionById(5).Returns(TestData.Edition());

			// Act
			var response = await client.GetAsync("/api/Edition/5", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertEditionDetailsJson(await ReadJson(response));
		}

		[Fact]
		public async Task FindById_NonExistentId_Returns404ProblemDetails()
		{
			// Arrange
			factory.EditionsService.GetEditionById(99).Returns((Edition)null);

			// Act
			var response = await client.GetAsync("/api/Edition/99", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
			Assert.Equal(404, (await ReadJson(response)).GetProperty("status").GetInt32());
		}

		[Fact]
		public async Task Create_ValidEdition_Returns201WithLocation()
		{
			// Arrange
			factory.EditionsService.When(s => s.CreateCourseEdition(Arg.Any<Edition>())).Do(call => call.Arg<Edition>().Id = 5);

			// Act
			var response = await client.PostAsJsonAsync("/api/Edition", EditionDetailsDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal("/api/edition/5", response.Headers.Location?.OriginalString);
			var json = await ReadJson(response);
			Assert.Equal(5, json.GetProperty("id").GetInt64());
			Assert.Equal("2026-03-02", json.GetProperty("startDate").GetString());
		}

		[Fact]
		public async Task Create_RelatedEntityNotFound_Returns400WithErrorObject()
		{
			// Arrange
			factory.EditionsService.CreateCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException("Course not found", nameof(Course)));

			// Act
			var response = await client.PostAsJsonAsync("/api/Edition", EditionDetailsDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(400, json.GetProperty("statusCode").GetInt32());
			Assert.Equal("Course not found", json.GetProperty("message").GetString());
		}

		[Fact]
		public async Task Edit_ValidEdition_Returns200WithBody()
		{
			// Act
			var response = await client.PutAsJsonAsync("/api/Edition", EditionDetailsDTO(5), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(5, json.GetProperty("id").GetInt64());
			Assert.Equal("CS-2026-01", json.GetProperty("code").GetString());
			factory.EditionsService.Received(1).EditCourseEdition(Arg.Is<Edition>(e => e.Id == 5));
		}

		[Theory]
		[InlineData(nameof(Course))]
		[InlineData(nameof(Instructor))]
		public async Task Edit_RelatedEntityNotFound_Returns400WithErrorObject(string entityName)
		{
			// Arrange
			factory.EditionsService.EditCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException($"{entityName} not found", entityName));

			// Act
			var response = await client.PutAsJsonAsync("/api/Edition", EditionDetailsDTO(5), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(400, json.GetProperty("statusCode").GetInt32());
			Assert.Equal($"{entityName} not found", json.GetProperty("message").GetString());
		}

		[Fact] // Expected red: the 404 body is a plain string
		public async Task Edit_EditionNotFound_Returns404WithErrorObject()
		{
			// Arrange
			factory.EditionsService.EditCourseEdition(Arg.Any<Edition>()).Throws(new EntityNotFoundException("Edition not found", nameof(Edition)));

			// Act
			var response = await client.PutAsJsonAsync("/api/Edition", EditionDetailsDTO(99), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
			var json = await ReadJson(response);
			Assert.Equal(404, json.GetProperty("statusCode").GetInt32());
			Assert.Equal("Edition not found", json.GetProperty("message").GetString());
		}

		[Fact]
		public async Task Delete_ExistingId_Returns204()
		{
			// Act
			var response = await client.DeleteAsync("/api/Edition/5", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
			factory.EditionsService.Received(1).DeleteCourseEdition(5);
		}

		[Fact]
		public async Task Delete_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange
			factory.EditionsService.When(s => s.DeleteCourseEdition(99)).Throw(new EntityNotFoundException("Edition not found", nameof(Edition)));

			// Act
			var response = await client.DeleteAsync("/api/Edition/99", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(404, json.GetProperty("statusCode").GetInt32());
			Assert.Equal("Edition not found", json.GetProperty("message").GetString());
		}

		[Fact]
		public async Task GetEditionsByCourseId_WithEditions_Returns200WithEditionDetails()
		{
			// Arrange
			factory.EditionsService.GetEditionsByCourseId(1).Returns(new List<Edition> { TestData.Edition() });

			// Act
			var response = await client.GetAsync("/api/Edition/course/1", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertEditionDetailsJson(Assert.Single((await ReadJson(response)).EnumerateArray()));
			factory.EditionsService.Received(1).GetEditionsByCourseId(1);
		}

		[Fact]
		public async Task GetEditionsByCourseId_NoEditions_Returns200WithEmptyArray()
		{
			// Arrange
			factory.EditionsService.GetEditionsByCourseId(1).Returns(new List<Edition>());

			// Act
			var response = await client.GetAsync("/api/Edition/course/1", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}

		[Fact]
		public async Task GetAvailableEnrollmentByStudentId_WithEditions_Returns200WithEditionDetails()
		{
			// Arrange
			factory.EditionsService.GetAvailableEnrollmentByStudentId(4).Returns(new List<Edition> { TestData.Edition() });

			// Act
			var response = await client.GetAsync("/api/Edition/studentAvailable/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertEditionDetailsJson(Assert.Single((await ReadJson(response)).EnumerateArray()));
			factory.EditionsService.Received(1).GetAvailableEnrollmentByStudentId(4);
		}

		[Fact]
		public async Task GetAvailableEnrollmentByStudentId_NoEditions_Returns200WithEmptyArray()
		{
			// Arrange
			factory.EditionsService.GetAvailableEnrollmentByStudentId(4).Returns(new List<Edition>());

			// Act
			var response = await client.GetAsync("/api/Edition/studentAvailable/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}
	}
}
