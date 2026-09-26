using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using TaskTrack.Repo.Models;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _tagService.GetAllTagsAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _tagService.CreateTagAsync(tag);
            return CreatedAtAction(nameof(GetAll), new { id = created.TagId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Tag tag)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _tagService.UpdateTagAsync(id, tag);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
