using AcademyModel.Entities;
using AcademyModel.Exceptions;
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
	public class EnrollmentController : Controller
	{
		private IEnrollmentsService service;
		private IMapper mapper;
		public EnrollmentController(IEnrollmentsService service, IMapper mapper)
		{
			this.service = service;
			this.mapper = mapper;
		}
		/// <summary>Returns the enrollments of the student with the given id.</summary>
		/// <param name="id">Student id.</param>
		/// <response code="200">List of enrollments.</response>
		[ProducesResponseType(typeof(IEnumerable<EnrollmentDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		[Route("studentSubscribed/{id}")]
		public IActionResult GetEnrollmentByStudentId(long id)
		{
			var enrollments = service.GetSubscribedEnrollmentByStudentId(id);
			var enrollmentDTOs = mapper.Map<IEnumerable<EnrollmentDTO>>(enrollments);
			return Ok(enrollmentDTOs);
		}
        //[HttpGet]
        //[Route("studentAvailable/{id}")]
        //public IActionResult GetAvailableEnrollmentByStudentId(long id)
        //{
        //    var enrollments = service.GetAvailableEnrollmentByStudentId(id);
        //    var enrollmentDTOs = mapper.Map<IEnumerable<EditionDetailsDTO>>(enrollments);
        //    return Ok(enrollmentDTOs);
        //}
		/// <summary>Creates a new enrollment.</summary>
		/// <param name="e">Enrollment to create.</param>
		/// <response code="201">The created enrollment.</response>
		/// <response code="400">Student or edition not found.</response>
		[ProducesResponseType(typeof(EnrollmentDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ErrorObject), StatusCodes.Status400BadRequest)]
		[HttpPost]
		public IActionResult Create(EnrollmentDTO e)
		{
			try
			{
				var enrollment = mapper.Map<Enrollment>(e);
				service.CreateEnrollment(enrollment);
				var enrollmentDTO = mapper.Map<EnrollmentDTO>(enrollment);
				return Created($"/api/edition/{enrollmentDTO.Id}", enrollmentDTO);
			}
			catch (EntityNotFoundException ex)
			{
				return BadRequest(new ErrorObject(StatusCodes.Status400BadRequest, ex.Message));
			}
		}
		/// <summary>Deletes the enrollment with the given id.</summary>
		/// <param name="id">Enrollment id.</param>
		/// <response code="200">Enrollment deleted.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[HttpDelete]
		[Route("{id}")]
		public IActionResult DeleteEnroll(long id)
		{
			service.DeleteEnrollment(id);
			return Ok();
		}
	}
	
}
