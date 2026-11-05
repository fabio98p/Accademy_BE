using AcademyModel.BuisnessLogic;
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
	public class StudentController : Controller
	{
		private IPeopleService service;
		private IEnrollmentsService enrollmentsService;
		private IMapper mapper;
		public StudentController(IPeopleService service, IMapper mapper)
		{
			this.service = service;
			this.mapper = mapper;
		}

		/// <summary>Returns all the students.</summary>
		/// <response code="200">List of students.</response>
		[ProducesResponseType(typeof(IEnumerable<StudentDTO>), StatusCodes.Status200OK)]
		[HttpGet]
		public IActionResult GetAll()
		{
			//var students = service.GetAllStudents();
			//var studentDTOS = students.Select(s => new StudentDTO(s));
			//return studentDTOS;

			var students = service.GetAllStudents();
			var studentDTOs = mapper.Map<IEnumerable<StudentDTO>>(students);
			return Ok(studentDTOs);
		}
		/// <summary>Updates an existing student.</summary>
		/// <param name="s">Student with the updated data.</param>
		/// <response code="201">The updated student.</response>
		[ProducesResponseType(typeof(StudentDTO), StatusCodes.Status201Created)]
		[HttpPut]
		public IActionResult UpdateStudent(StudentDTO s)
		{
			var student = mapper.Map<Student>(s);
			service.UpdateStudent(student);
			var studentDTO = mapper.Map<StudentDTO>(student);
			return Created($"/api/student/{studentDTO.Id}", studentDTO);
		}
		/// <summary>Creates a new student.</summary>
		/// <param name="s">Student to create.</param>
		/// <response code="201">The created student.</response>
		[ProducesResponseType(typeof(StudentDTO), StatusCodes.Status201Created)]
		[HttpPost]
		public IActionResult Create(StudentDTO s)
		{
			var student = mapper.Map<Student>(s);
			service.CreateStudent(student);
			var studentDTO = mapper.Map<StudentDTO>(student);
			return Created($"/api/student/{studentDTO.Id}", studentDTO);
		}

		/// <summary>Returns the student with the given id.</summary>
		/// <param name="id">Student id.</param>
		/// <response code="200">The student (empty body if it does not exist).</response>
		[ProducesResponseType(typeof(StudentDTO), StatusCodes.Status200OK)]
		[HttpGet]
		[Route("{id}")]
		public IActionResult GetStudentById(long id)
		{
			var student = service.GetStudentById(id);
			var studentDTO = mapper.Map<StudentDTO>(student);
			return Ok(studentDTO);
		}

		/// <summary>Enrolls the student in a course edition.</summary>
		/// <param name="dataDTO">Student and edition ids.</param>
		/// <param name="idStudent">Student id; must match the id in the body.</param>
		/// <response code="201">The created enrollment.</response>
		/// <response code="400">The student id in the URL and in the body do not match.</response>
		[ProducesResponseType(typeof(EnrollmentDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ErrorObject), StatusCodes.Status400BadRequest)]
		[HttpPost]
		[Route("{idStudent}/enrollments")]
		public IActionResult EnrollStudent(EnrollDataDTO dataDTO, long idStudent)
		{
			if (idStudent != dataDTO.IdStudent)
			{
				return BadRequest(new ErrorObject(StatusCodes.Status400BadRequest, "L'id studente nell'URL e nel body non coincidono."));
			}
			var data = mapper.Map<EnrollData>(dataDTO);
			var enr = enrollmentsService.EnrollSudentToEdition(data);
			var enrDTO = mapper.Map<EnrollmentDTO>(enr);
			return Created($"/api/student/{data.IdStudent}/enrollments/{enr.Id}", enrDTO);
		}
		/// <summary>Deletes the student with the given id.</summary>
		/// <param name="id">Student id.</param>
		/// <response code="200">The student read after the deletion (entity, not DTO).</response>
		[ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
		[HttpDelete]
		[Route("{id}")]
		public IActionResult DeleteStudent(long id)
		{
			service.DeleteStudent(id);
			var s = service.GetStudentById(id);
			var studentDTO = mapper.Map<Student>(s);
			return Ok(studentDTO);
		}
	}
}
