using AcademyModel.Entities;
using AcademyModel.Services;
using AutoMapper;
using CodeAcademyWeb.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CodeAcademyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : Controller
    {
        private ILessonsService service;
        private IMapper mapper;
        public LessonController(ILessonsService service, IMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }
        /// <summary>Returns the lessons of the edition with the given id.</summary>
        /// <param name="id">Edition id.</param>
        /// <response code="200">List of lessons.</response>
        [ProducesResponseType(typeof(IEnumerable<LessonDTO>), StatusCodes.Status200OK)]
        [Route("lessonByEdition/{id}")]
        [HttpGet]
        public IActionResult FindLessonForEditionId(long id)
        {
            var lessons = service.FindLessonForEditionId(id);
            var lessonDTOs = mapper.Map<IEnumerable<LessonDTO>>(lessons);

            return Ok(lessonDTOs);
        }

        //serve passargli 2 date
        //[HttpGet]
        //public IActionResult FindLessonInRange()
        //{
        //    var areas = service.FindLessonInRange();
        //    var areaDTOs = mapper.Map<IEnumerable<AreaDTO>>(areas);
        //    return Ok(areaDTOs);
        //}
    }
}
