using AutoMapper;
using CodeAcademyWeb.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeAcademyWeb.Tests.Support
{
	/// <summary>Builds the real <see cref="IMapper"/> from the profiles of the CodeAcademyWeb assembly.</summary>
	public static class MapperFactory
	{
		public static IMapper Create()
		{
			var configuration = new MapperConfiguration(
				cfg => cfg.AddMaps(typeof(AreaProfile).Assembly),
				NullLoggerFactory.Instance);
			return configuration.CreateMapper();
		}
	}
}
