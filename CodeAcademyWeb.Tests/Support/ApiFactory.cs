using AcademyModel.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Text.Json;

namespace CodeAcademyWeb.Tests.Support
{
	/// <summary>
	/// In-memory host of the API: the services are replaced with NSubstitute mocks, so no database is used.
	/// Create one instance per test to avoid shared state.
	/// </summary>
	public class ApiFactory : WebApplicationFactory<Startup>
	{
		public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

		public IAreasService AreasService { get; } = Substitute.For<IAreasService>();
		public ICoursesService CoursesService { get; } = Substitute.For<ICoursesService>();
		public IEditionsService EditionsService { get; } = Substitute.For<IEditionsService>();
		public IEnrollmentsService EnrollmentsService { get; } = Substitute.For<IEnrollmentsService>();
		public ILessonsService LessonsService { get; } = Substitute.For<ILessonsService>();
		public IPeopleService PeopleService { get; } = Substitute.For<IPeopleService>();

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureTestServices(services =>
			{
				services.AddSingleton(AreasService);
				services.AddSingleton(CoursesService);
				services.AddSingleton(EditionsService);
				services.AddSingleton(EnrollmentsService);
				services.AddSingleton(LessonsService);
				services.AddSingleton(PeopleService);
			});
		}
	}
}
