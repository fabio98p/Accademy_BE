using AcademyEFPersistance.EFContext;
using AcademyEFPersistance.Repository;
using AcademyEFPersistance.Services;
using AcademyModel.Repositories;
using AcademyModel.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace CodeAcademyWeb
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<AcademyContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("AcademyDb"), x => x.UseNodaTime())
						   .LogTo(Console.WriteLine, new[]
						   {
			  DbLoggerCategory.Database.Command.Name
						   }, LogLevel.Information).EnableSensitiveDataLogging());

			services.AddControllersWithViews();
			services.AddOpenApi();

			services.AddScoped<IAreasService, EFAreasService>();
			services.AddScoped<ICoursesService, EFCoursesService>();
			services.AddScoped<IEditionsService, EFEditionsService>();
			services.AddScoped<IEnrollmentsService, EFEnrollmentsService>();
			services.AddScoped<ILessonsService, EFLessonsService>();
			services.AddScoped<IPeopleService, EFPeopleService>();

            services.AddScoped<IStudentRepository, EFStudentRepository>();
			services.AddScoped<ICourseRepository, EFCourseRepository>();
			services.AddScoped<IEditionRepository, EFEditionRepository>();
			services.AddScoped<IInstructorRepository, EFInstructorRepository>();
			services.AddScoped<IAreaRepository, EFAreaRepository>();
			services.AddScoped<ILessonRepository, EFLessonRepository>();
			services.AddScoped<IEnrollmentRepository, EFEnrollmentRepository>();

            services.AddCors(c =>                            // permette chiamate CORS per front-end
			{
				c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
			});


			services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies()); //configurazione auto mapper per casting a DTO
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				//app.UseDeveloperExceptionPage();
			}
			else
			{
				//app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			if (env.IsDevelopment())
			{
				app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
				//endpoints.MapControllers();
				if (env.IsDevelopment())
				{
					endpoints.MapOpenApi();
				}
			});
		}
	}
}
