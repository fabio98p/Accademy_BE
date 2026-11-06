using AcademyModel;
using AcademyModel.Entities;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
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
	public class CourseHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public CourseHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
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

		private async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
			TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

		private static void AssertCourseJson(JsonElement course)
		{
			Assert.Equal(1, course.GetProperty("id").GetInt64());
			Assert.Equal("C# Fundamentals", course.GetProperty("title").GetString());
			Assert.Equal((int)Level.INTERMEDIATE, course.GetProperty("level").GetInt32());
			Assert.Equal(2, course.GetProperty("areaId").GetInt64());
			Assert.Equal("Development", course.GetProperty("areaName").GetString());
			Assert.True(course.GetProperty("grantsCertification").GetBoolean());
			Assert.Equal("2026-01-10", course.GetProperty("creationDate").GetString());
		}

		private void ServiceCreatesWithId(long id)
		{
			factory.CoursesService.CreateCourse(Arg.Any<Course>()).Returns(call =>
			{
				var course = call.Arg<Course>();
				course.Id = id;
				return course;
			});
		}

		private async Task AssertNotFoundErrorObject(HttpResponseMessage response)
		{
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Equal(404, (await ReadJson(response)).GetProperty("statusCode").GetInt32());
		}

		[Fact]
		public async Task GetAllCourse_WithCourses_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.CoursesService.GetAllCourses().Returns(new List<Course> { TestData.Course() });

			// Act
			var response = await client.GetAsync("/api/Course", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertCourseJson(Assert.Single((await ReadJson(response)).EnumerateArray()));
		}

		[Fact]
		public async Task GetAllCourse_NoCourses_Returns200WithEmptyArray()
		{
			// Arrange
			factory.CoursesService.GetAllCourses().Returns(new List<Course>());

			// Act
			var response = await client.GetAsync("/api/Course", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}

		[Fact]
		public async Task GetLastNCurses_RouteN_BindsNToService()
		{
			// Arrange
			factory.CoursesService.GetLastCourses(3).Returns(new List<Course> { TestData.Course() });

			// Act
			var response = await client.GetAsync("/api/Course/lastCourse/3", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertCourseJson(Assert.Single((await ReadJson(response)).EnumerateArray()));
			factory.CoursesService.Received(1).GetLastCourses(3);
		}

		[Fact]
		public async Task GetLastNCurses_NoCourses_Returns200WithEmptyArray()
		{
			// Arrange
			factory.CoursesService.GetLastCourses(3).Returns(new List<Course>());

			// Act
			var response = await client.GetAsync("/api/Course/lastCourse/3", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty((await ReadJson(response)).EnumerateArray());
		}

		[Fact]
		public async Task GetCourseById_ExistingId_Returns200WithCourse()
		{
			// Arrange
			factory.CoursesService.GetCourseById(1).Returns(TestData.Course());

			// Act
			var response = await client.GetAsync("/api/Course/1", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertCourseJson(await ReadJson(response));
		}

		[Fact] // Expected red: returns 204 No Content
		public async Task GetCourseById_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange
			factory.CoursesService.GetCourseById(99).Returns((Course)null);

			// Act
			var response = await client.GetAsync("/api/Course/99", TestContext.Current.CancellationToken);

			// Assert
			await AssertNotFoundErrorObject(response);
		}

		[Fact]
		public async Task CreateCourse_ValidCourse_Returns201WithBody()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var response = await client.PostAsJsonAsync("/api/Course", CourseDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(1, json.GetProperty("id").GetInt64());
			Assert.Equal("C# Fundamentals", json.GetProperty("title").GetString());
			factory.CoursesService.Received(1).CreateCourse(Arg.Is<Course>(c => c.Title == "C# Fundamentals" && c.AreaId == 2));
		}

		[Fact] // Expected red: Location points to /api/courses/{id}
		public async Task CreateCourse_ValidCourse_LocationPointsToCourse()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var response = await client.PostAsJsonAsync("/api/Course", CourseDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal("/api/Course/1", response.Headers.Location?.OriginalString);
		}

		[Fact] // Expected red: the creation date is not set server-side
		public async Task CreateCourse_WithoutCreationDate_SetsTodayAsCreationDate()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var response = await client.PostAsJsonAsync("/api/Course", CourseDTO(creationDate: null), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal(TestData.Today, (await ReadJson(response)).GetProperty("creationDate").GetString());
		}

		[Fact] // Expected red: the creation date sent by the client is kept
		public async Task CreateCourse_WithClientCreationDate_SetsTodayAsCreationDate()
		{
			// Arrange
			ServiceCreatesWithId(1);

			// Act
			var response = await client.PostAsJsonAsync("/api/Course", CourseDTO(creationDate: "2020-05-01"), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal(TestData.Today, (await ReadJson(response)).GetProperty("creationDate").GetString());
		}

		[Fact] // Expected red: returns 201 Created
		public async Task UpdateCourse_ValidCourse_Returns200WithBody()
		{
			// Arrange
			factory.CoursesService.UpdateCourse(Arg.Any<Course>()).Returns(call => call.Arg<Course>());

			// Act
			var response = await client.PutAsJsonAsync("/api/Course", CourseDTO(1), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(1, json.GetProperty("id").GetInt64());
			Assert.Equal("C# Fundamentals", json.GetProperty("title").GetString());
			factory.CoursesService.Received(1).UpdateCourse(Arg.Is<Course>(c => c.Id == 1));
		}

		[Fact]
		public async Task RemoveCourse_WithBody_Returns200WithBodyAndDeletesCourse()
		{
			// Arrange
			var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Course")
			{
				Content = JsonContent.Create(CourseDTO(1))
			};

			// Act
			var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = await ReadJson(response);
			Assert.Equal(1, json.GetProperty("id").GetInt64());
			Assert.Equal("C# Fundamentals", json.GetProperty("title").GetString());
			factory.CoursesService.Received(1).DeleteCourse(Arg.Is<Course>(c => c.Id == 1));
		}

		[Fact]
		public async Task RemoveCourse_ExistingId_Returns200WithBodyAndDeletesCourse()
		{
			// Arrange
			factory.CoursesService.GetCourseById(1).Returns(TestData.Course());

			// Act
			var response = await client.DeleteAsync("/api/Course/1", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			AssertCourseJson(await ReadJson(response));
			factory.CoursesService.Received(1).DeleteCourse(1L);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public async Task RemoveCourse_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			factory.CoursesService.GetCourseById(99).Returns((Course)null);
			factory.CoursesService.When(s => s.DeleteCourse(99L)).Throw(new ArgumentNullException("entity"));

			// Act
			var response = await client.DeleteAsync("/api/Course/99", TestContext.Current.CancellationToken);

			// Assert
			await AssertNotFoundErrorObject(response);
		}
	}
}
