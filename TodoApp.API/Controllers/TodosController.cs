using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApp.API.Models;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly TodoRepository _todoRepo;
    private readonly ProfileRepository _profileRepo;

    public TodosController(TodoRepository todoRepo, ProfileRepository profileRepo)
    {
        _todoRepo = todoRepo;
        _profileRepo = profileRepo;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
        return Guid.Parse(userIdClaim);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var items = await _todoRepo.GetAllForProfileAsync(userId);
        var dtos = items.Select(i => new TodoItemDto
        {
            Id = i.Id,
            Text = i.Text,
            Status = i.Status,
            LastUpdate = i.LastUpdate
        });
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var item = await _todoRepo.GetByIdAsync(id);
        if (item == null || item.ProfileId != userId)
            return NotFound();

        return Ok(new TodoItemDto
        {
            Id = item.Id,
            Text = item.Text,
            Status = item.Status,
            LastUpdate = item.LastUpdate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTodoDto dto)
    {
        var userId = GetUserId();
        var item = new TodoItem(dto.Text, dto.Status, DateTime.Now)
        {
            ProfileId = userId
        };
        await _todoRepo.AddAsync(item);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, new TodoItemDto
        {
            Id = item.Id,
            Text = item.Text,
            Status = item.Status,
            LastUpdate = item.LastUpdate
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTodoDto dto)
    {
        var userId = GetUserId();
        var item = await _todoRepo.GetByIdAsync(id);
        if (item == null || item.ProfileId != userId)
            return NotFound();

        if (dto.Text != null)
            item.Text = dto.Text;
        if (dto.Status.HasValue)
            item.Status = dto.Status.Value;
        item.LastUpdate = DateTime.Now;

        await _todoRepo.UpdateAsync(item);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var item = await _todoRepo.GetByIdAsync(id);
        if (item == null || item.ProfileId != userId)
            return NotFound();

        await _todoRepo.DeleteAsync(id);
        return NoContent();
    }
}