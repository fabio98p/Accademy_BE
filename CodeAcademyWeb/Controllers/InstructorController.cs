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
	public class InstructorController : ControllerBase
	{
		private IPeopleService service;
		private IMapper mapper;
		public InstructorController(IPeopleService service, IMapper mapper)
		{
			this.service = service;
			this.mapper = mapper;
		}
		/// <summary>Returns all the instructors.</summary>
		/// <response code="200">List of instructors.</response>
		[ProducesResponseType(typeof(IEnumerable<InstructorDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		public IActionResult GetAllInstructor()
		{
			var instructor = service.GetInstructors();
			var instructorDTO = mapper.Map<IEnumerable<InstructorDTO>>(instructor);
			return Ok(instructorDTO);
		}
	}
}
