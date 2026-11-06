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
	public class AreaHttpTests : IDisposable
	{
		private readonly ApiFactory factory = new();
		private readonly HttpClient client;

		public AreaHttpTests()
		{
			client = factory.CreateClient();
		}

		public void Dispose()
		{
			client.Dispose();
			factory.Dispose();
		}

		[Fact]
		public async Task GetAllAreas_WithAreas_Returns200WithCamelCaseJson()
		{
			// Arrange
			factory.AreasService.GetAllAreas().Returns(new List<Area>
			{
				new Area { Id = 1, Name = "Development", Description = "Software development" }
			});

			// Act
			var response = await client.GetAsync("/api/Area/areas", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
			var area = Assert.Single(json.RootElement.EnumerateArray());
			Assert.Equal(1, area.GetProperty("id").GetInt64());
			Assert.Equal("Development", area.GetProperty("name").GetString());
			Assert.Equal("Software development", area.GetProperty("description").GetString());
		}

		[Fact]
		public async Task GetAllAreas_NoAreas_Returns200WithEmptyArray()
		{
			// Arrange
			factory.AreasService.GetAllAreas().Returns(new List<Area>());

			// Act
			var response = await client.GetAsync("/api/Area/areas", TestContext.Current.CancellationToken);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var areas = await response.Content.ReadFromJsonAsync<List<AreaDTO>>(ApiFactory.Json, TestContext.Current.CancellationToken);
			Assert.Empty(areas);
		}
	}
}
