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
    public class AreaController : Controller
    {
        private IAreasService service;
        private IMapper mapper;
        public AreaController(IAreasService service, IMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }
        /// <summary>Returns all the areas.</summary>
        /// <response code="200">List of areas.</response>
        [ProducesResponseType(typeof(IEnumerable<AreaDTO>), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("areas")]
        public IActionResult GetAllAreas()
        {
            var areas = service.GetAllAreas();
            var areaDTOs = mapper.Map<IEnumerable<AreaDTO>>(areas);
            return Ok(areaDTOs);
        }
    }
}
