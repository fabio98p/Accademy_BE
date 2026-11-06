using AcademyModel.Entities;
using CodeAcademyWeb.DTOs;
using CodeAcademyWeb.Tests.Support;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CodeAcademyWeb.Tests.Http
{
	[Trait("Category", "Http")]
	public class LessonHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public LessonHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
		}

		[Fact]
		public async Task FindLessonForEditionId_WithLessons_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.LessonsService.FindLessonForEditionId(7).Returns(new List<Lesson>
			{
				new Lesson { Id = 1, Subject = "Introduction", CourseEditionId = 7, ClassroomId = 3 }
			});

			// Act
			var response = await client.GetAsync("/api/Lesson/lessonByEdition/7", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			var lesson = Assert.Single(json.EnumerateArray());
			Assert.Equal(1, lesson.GetProperty("id").GetInt64());
			Assert.Equal("Introduction", lesson.GetProperty("subject").GetString());
			Assert.Equal(7, lesson.GetProperty("courseEditionId").GetInt64());
			Assert.Equal(3, lesson.GetProperty("classroomId").GetInt64());
		}

		[Fact]
		public async Task FindLessonForEditionId_RouteId_BindsIdToService()
		{
			// Arrange
			factory.LessonsService.FindLessonForEditionId(Arg.Any<long>()).Returns(new List<Lesson>());

			// Act
			await client.GetAsync("/api/Lesson/lessonByEdition/42", TestContext.Current.CancellationToken);

			// Assert
			factory.LessonsService.Received(1).FindLessonForEditionId(42);
		}

		[Fact]
		public async Task FindLessonForEditionId_NoLessons_Returns200WithEmptyArray()
		{
			// Arrange
			factory.LessonsService.FindLessonForEditionId(7).Returns(new List<Lesson>());

			// Act
			var response = await client.GetAsync("/api/Lesson/lessonByEdition/7", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var lessons = await response.Content.ReadFromJsonAsync<List<LessonDTO>>(ApiFactory.Json, TestContext.Current.CancellationToken);
			Assert.Empty(lessons);
		}

		[Fact]
		public async Task FindLessonForEditionId_NonNumericId_Returns400()
		{
			// Act
			var response = await client.GetAsync("/api/Lesson/lessonByEdition/abc", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			factory.LessonsService.DidNotReceive().FindLessonForEditionId(Arg.Any<long>());
		}
	}
}
