using AcademyModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademyEFPersistance.EFContext
{
	public class AcademyContext : DbContext
	{
		public DbSet<Area> Areas { get; set; }
		public DbSet<Classroom> Classrooms { get; set; }
		public DbSet<Competence> Competences { get; set; }
		public DbSet<Course> Courses { get; set; }
		public DbSet<Edition> CourseEditions { get; set; }
		public DbSet<Instructor> Instructors { get; set; }
		public DbSet<Lesson> Lessons { get; set; }
		public DbSet<Skill> Skills { get; set; }
		public DbSet<Student> Students { get; set; }
		public DbSet<Enrollment> Enrollments { get; set; }
		public AcademyContext() {}
		public AcademyContext(DbContextOptions<AcademyContext> options) : base(options) {}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<Student>()
				.HasMany(s => s.Enrollments)
				.WithOne(e => e.Student)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Course>()
				.Property(c => c.Title)
				.IsRequired();
			modelBuilder.Entity<Person>()
				.Property(c => c.Firstname)
				.IsRequired();
			modelBuilder.Entity<Person>()
				.Property(c => c.Lastname)
				.IsRequired();
			modelBuilder.Entity<Enrollment>()
				.HasIndex(e => new { e.StudentId, e.CourseEditionId }).IsUnique();

			modelBuilder.Entity<Course>()
				.Property(c => c.BasePrice)
				.HasPrecision(18, 2);
			modelBuilder.Entity<Instructor>()
				.Property(i => i.PayRate)
				.HasPrecision(18, 2);
			modelBuilder.Entity<Edition>()
				.Property(e => e.RealPrice)
				.HasPrecision(18, 2);
		}
	}
}
