using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoTask>>> GetAll()
    {
        var userId = GetUserId();
        return Ok(await dbContext.Tasks.Where(t => t.UserId == userId).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoTask>> GetById(int id)
    {
        var userId = GetUserId();
        var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TodoTask>> Create(TaskRequest request)
    {
        var task = new TodoTask
        {
            Title = request.Title,
            Content = request.Content,
            Date = request.Date,
            Status = request.Status,
            UserId = GetUserId()
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskRequest request)
    {
        var userId = GetUserId();
        var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task is null)
        {
            return NotFound();
        }

        task.Title = request.Title;
        task.Content = request.Content;
        task.Date = request.Date;
        task.Status = request.Status;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task is null)
        {
            return NotFound();
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
