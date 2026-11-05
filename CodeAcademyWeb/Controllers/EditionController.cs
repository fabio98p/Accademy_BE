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
	public class EditionController : ControllerBase
	{
		private IEditionsService service;
        //private IDidactisService service;

        private IMapper mapper;
		public EditionController(IEditionsService service, IMapper mapper)
		{
			this.service = service;
			this.mapper = mapper;
		}
		/// <summary>Returns all the course editions.</summary>
		/// <response code="200">List of editions.</response>
		[ProducesResponseType(typeof(IEnumerable<EditionDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		public IActionResult GetAll()
		{
			var editions = service.GetAllEditions();
			var editionDTOs = mapper.Map<IEnumerable<EditionDTO>>(editions);
			return Ok(editionDTOs);
		}
		/// <summary>Returns the details of the edition with the given id.</summary>
		/// <param name="id">Edition id.</param>
		/// <response code="200">The edition details.</response>
		/// <response code="404">Edition not found.</response>
		[ProducesResponseType(typeof(EditionDetailsDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		[Route("{id}")]
		[HttpGet]
		public IActionResult FindById(long id)
		{
			var edition = service.GetEditionById(id);
			if (edition == null)
			{
				return NotFound();
			}
			var editionDTO = mapper.Map<EditionDetailsDTO>(edition);
			return Ok(editionDTO);
		}
		/// <summary>Creates a new course edition.</summary>
		/// <param name="e">Edition to create.</param>
		/// <response code="201">The created edition.</response>
		/// <response code="400">Course or instructor not found.</response>
		[ProducesResponseType(typeof(EditionDetailsDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ErrorObject), StatusCodes.Status400BadRequest)]
		[HttpPost]
		public IActionResult Create(EditionDetailsDTO e)
		{
			try
			{
				var edition = mapper.Map<Edition>(e);
				service.CreateCourseEdition(edition);
				var courseEditionDTO = mapper.Map<EditionDetailsDTO>(edition);
				return Created($"/api/edition/{courseEditionDTO.Id}", courseEditionDTO);
			}
			catch (EntityNotFoundException ex)
			{
				return BadRequest(new ErrorObject(StatusCodes.Status400BadRequest, ex.Message));
			}
		}
		/// <summary>Updates an existing course edition.</summary>
		/// <param name="e">Edition with the updated data.</param>
		/// <response code="200">The updated edition.</response>
		/// <response code="400">Course or instructor not found.</response>
		/// <response code="404">Edition not found (error message as plain string).</response>
		[ProducesResponseType(typeof(EditionDetailsDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ErrorObject), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
		[HttpPut]
		public IActionResult Edit(EditionDetailsDTO e)
		{
			try
			{
				var edition = mapper.Map<Edition>(e);
				service.EditCourseEdition(edition);
				var courseEditionDTO = mapper.Map<EditionDetailsDTO>(edition);
				return Ok(courseEditionDTO);
			}
			catch (EntityNotFoundException ex)
			{
				switch (ex.EntityName)
				{
					case nameof(Edition):
						return NotFound(ex.Message);

					default:
						return BadRequest(new ErrorObject(StatusCodes.Status400BadRequest, ex.Message));
				}
			}
		}
		/// <summary>Deletes the edition with the given id.</summary>
		/// <param name="id">Edition id.</param>
		/// <response code="204">Edition deleted.</response>
		/// <response code="404">Edition not found.</response>
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(ErrorObject), StatusCodes.Status404NotFound)]
		[Route("{id}")]
		[HttpDelete]
		public IActionResult Delete(long id)
		{
			try
			{
				service.DeleteCourseEdition(id);
				return NoContent();
			}
			catch (EntityNotFoundException ex)
			{
				return NotFound(new ErrorObject(StatusCodes.Status404NotFound, ex.Message));
			}
		}
		/// <summary>Returns the editions of the course with the given id.</summary>
		/// <param name="id">Course id.</param>
		/// <response code="200">List of editions.</response>
		[ProducesResponseType(typeof(IEnumerable<EditionDetailsDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		[Route("course/{id}")]
		public IActionResult GetEditionsByCourseId(long id)
		{
			var editions = service.GetEditionsByCourseId(id);
			var editionDTOs = mapper.Map<IEnumerable<EditionDetailsDTO>>(editions);
			return Ok(editionDTOs);
		}
        /// <summary>Returns the editions the student with the given id can enroll in.</summary>
        /// <param name="id">Student id.</param>
        /// <response code="200">List of available editions.</response>
        [ProducesResponseType(typeof(IEnumerable<EditionDetailsDTO>), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("studentAvailable/{id}")]
        public IActionResult GetAvailableEnrollmentByStudentId(long id)
        {
            var enrollments = service.GetAvailableEnrollmentByStudentId(id);
            var enrollmentDTOs = mapper.Map<IEnumerable<EditionDetailsDTO>>(enrollments);
            return Ok(enrollmentDTOs);
        }
    }
}
