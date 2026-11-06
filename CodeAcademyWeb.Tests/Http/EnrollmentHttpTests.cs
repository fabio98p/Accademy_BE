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
using System.Threading.Tasks;
using Xunit;

namespace CodeAcademyWeb.Tests.Http
{
	[Trait("Category", "Http")]
	public class EnrollmentHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public EnrollmentHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
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
			factory.EnrollmentsService.CreateEnrollment(Arg.Any<Enrollment>()).Returns(call =>
			{
				var enrollment = call.Arg<Enrollment>();
				enrollment.Id = id;
				return enrollment;
			});
		}

		[Fact]
		public async Task GetEnrollmentByStudentId_WithEnrollments_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.EnrollmentsService.GetSubscribedEnrollmentByStudentId(4).Returns(new List<Enrollment> { TestData.Enrollment() });

			// Act
			var response = await client.GetAsync("/api/Enrollment/studentSubscribed/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			var enrollment = Assert.Single(json.EnumerateArray());
			Assert.Equal(10, enrollment.GetProperty("id").GetInt64());
			Assert.Equal("Laura", enrollment.GetProperty("studentFirstname").GetString());
			Assert.Equal("CS-2026-01", enrollment.GetProperty("courseEditionCode").GetString());
			Assert.Equal("C# Fundamentals", enrollment.GetProperty("courseEditionCourseTitle").GetString());
			Assert.Equal("2026-03-02", enrollment.GetProperty("courseEditionStartDate").GetString());
		}

		[Fact]
		public async Task GetEnrollmentByStudentId_RouteId_BindsIdToService()
		{
			// Arrange
			factory.EnrollmentsService.GetSubscribedEnrollmentByStudentId(Arg.Any<long>()).Returns(new List<Enrollment>());

			// Act
			await client.GetAsync("/api/Enrollment/studentSubscribed/42", TestContext.Current.CancellationToken);

			// Assert
			factory.EnrollmentsService.Received(1).GetSubscribedEnrollmentByStudentId(42);
		}

		[Fact]
		public async Task GetEnrollmentByStudentId_NoEnrollments_Returns200WithEmptyArray()
		{
			// Arrange
			factory.EnrollmentsService.GetSubscribedEnrollmentByStudentId(4).Returns(new List<Enrollment>());

			// Act
			var response = await client.GetAsync("/api/Enrollment/studentSubscribed/4", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty(TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).EnumerateArray());
		}

		[Fact]
		public async Task Create_ValidEnrollment_Returns201WithBody()
		{
			// Arrange
			ServiceAssignsId(10);

			// Act
			var response = await client.PostAsJsonAsync("/api/Enrollment", NewEnrollmentDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var json = TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			Assert.Equal(10, json.GetProperty("id").GetInt64());
			Assert.Equal(4, json.GetProperty("studentId").GetInt64());
			Assert.Equal(5, json.GetProperty("courseEditionId").GetInt64());
			factory.EnrollmentsService.Received(1).CreateEnrollment(Arg.Is<Enrollment>(e => e.StudentId == 4 && e.CourseEditionId == 5));
		}

		[Fact] // Expected red: Location points to /api/edition/{id}
		public async Task Create_ValidEnrollment_LocationPointsToEnrollment()
		{
			// Arrange
			ServiceAssignsId(10);

			// Act
			var response = await client.PostAsJsonAsync("/api/Enrollment", NewEnrollmentDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal("/api/Enrollment/10", response.Headers.Location?.OriginalString);
		}

		[Fact]
		public async Task Create_RelatedEntityNotFound_Returns400WithErrorObject()
		{
			// Arrange
			factory.EnrollmentsService.CreateEnrollment(Arg.Any<Enrollment>()).Throws(new EntityNotFoundException("Student not found", nameof(Student)));

			// Act
			var response = await client.PostAsJsonAsync("/api/Enrollment", NewEnrollmentDTO(), TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var json = TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			Assert.Equal(400, json.GetProperty("statusCode").GetInt32());
			Assert.Equal("Student not found", json.GetProperty("message").GetString());
		}

		[Fact]
		public async Task DeleteEnroll_ExistingId_Returns200WithoutBody()
		{
			// Act
			var response = await client.DeleteAsync("/api/Enrollment/10", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			factory.EnrollmentsService.Received(1).DeleteEnrollment(10);
		}

		[Fact] // Expected red: ArgumentNullException is not handled (500)
		public async Task DeleteEnroll_NonExistentId_Returns404WithErrorObject()
		{
			// Arrange: the EF repository calls Remove(null) when the id does not exist
			factory.EnrollmentsService.When(s => s.DeleteEnrollment(99)).Throw(new ArgumentNullException("entity"));

			// Act
			var response = await client.DeleteAsync("/api/Enrollment/99", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			var json = TestData.ParseJson(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			Assert.Equal(404, json.GetProperty("statusCode").GetInt32());
		}
	}
}
