using AcademyModel.Entities;
using AcademyModel.Services;
using AutoMapper;
using CodeAcademyWeb.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademyWeb.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CourseController : ControllerBase
	{
		//private IDidactisService service;
		private ICoursesService service;
        private IMapper mapper;
		public CourseController(ICoursesService service, IMapper mapper)
		{
			this.service = service;
			this.mapper = mapper;
		}
		/// <summary>Returns all the courses.</summary>
		/// <response code="200">List of courses.</response>
		[ProducesResponseType(typeof(IEnumerable<CourseDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		public IActionResult GetAllCourse()
		{
			var course = service.GetAllCourses();
			var courseDTOs = mapper.Map<IEnumerable<CourseDTO>>(course);
			return Ok(courseDTOs);
		}
		/// <summary>Returns the course with the given id.</summary>
		/// <param name="id">Course id.</param>
		/// <response code="200">The course (empty body if it does not exist).</response>
		[ProducesResponseType(typeof(CourseDTO), StatusCodes.Status200OK)]
		[HttpGet]
		[Route("{id}")]
		public IActionResult GetCourseById(long id)
		{
			var course = service.GetCourseById(id);
			var courseDTO = mapper.Map<CourseDTO>(course);
			return Ok(courseDTO);
		}
		/// <summary>Creates a new course.</summary>
		/// <param name="courseDTO">Course to create.</param>
		/// <response code="201">The created course.</response>
		[ProducesResponseType(typeof(CourseDTO), StatusCodes.Status201Created)]
		[HttpPost]
		public IActionResult CreateCourse(CourseDTO courseDTO)
		{
			var course = mapper.Map<Course>(courseDTO);
			course = service.CreateCourse(course);
			var resDTO = mapper.Map<CourseDTO>(course);
			return Created($"/api/courses/{resDTO.Id}", resDTO);
		}
		/// <summary>Updates an existing course.</summary>
		/// <param name="courseDTO">Course with the updated data.</param>
		/// <response code="201">The updated course.</response>
		[ProducesResponseType(typeof(CourseDTO), StatusCodes.Status201Created)]
		[HttpPut]
		public IActionResult UpdateCourse(CourseDTO courseDTO)
		{
			var course = mapper.Map<Course>(courseDTO);
			course = service.UpdateCourse(course);
			var resDTO = mapper.Map<CourseDTO>(course);
			return Created($"/api/courses/{resDTO.Id}", resDTO);
		}
		/// <summary>Deletes the course passed in the body.</summary>
		/// <param name="courseDTO">Course to delete.</param>
		/// <response code="200">The deleted course.</response>
		[ProducesResponseType(typeof(CourseDTO), StatusCodes.Status200OK)]
		[HttpDelete]
		public IActionResult RemoveCourse(CourseDTO courseDTO)
		{
			var course = mapper.Map<Course>(courseDTO);
			service.DeleteCourse(course);
			var resDTO = mapper.Map<CourseDTO>(course);
			return Ok(resDTO);
		}

		/// <summary>Deletes the course with the given id.</summary>
		/// <param name="id">Course id.</param>
		/// <response code="200">The deleted course.</response>
		[ProducesResponseType(typeof(CourseDTO), StatusCodes.Status200OK)]
		[HttpDelete]
		[Route("{id}")]
		public IActionResult RemoveCourse(long id)
		{
			var course = service.GetCourseById(id);
			service.DeleteCourse(id);
			var resDTO = mapper.Map<CourseDTO>(course);
			return Ok(resDTO);
		}
		/// <summary>Returns the last n created courses.</summary>
		/// <param name="n">Number of courses to return.</param>
		/// <response code="200">List of courses.</response>
		[ProducesResponseType(typeof(IEnumerable<CourseDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		[Route("lastCourse/{n}")]
		public IActionResult GetLastNCurses(int n)
		{
			var courses = service.GetLastCourses(n);

			var courseDTOs = mapper.Map<IEnumerable<CourseDTO>>(courses);
			return Ok(courseDTOs);
		}
	}
}
