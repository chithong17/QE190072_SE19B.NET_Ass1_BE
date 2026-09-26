using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using TaskTrack.Repo.Models;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _departmentService.GetAllActiveDepartmentsAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _departmentService.GetDepartmentByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            var data = await _departmentService.SearchDepartmentsByNameAsync(name ?? string.Empty);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _departmentService.CreateDepartmentAsync(department);
            return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Department department)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _departmentService.UpdateDepartmentAsync(id, department);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _departmentService.DeleteDepartmentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
