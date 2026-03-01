using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext dbContext, TokenService tokenService) : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return BadRequest("Email já cadastrado.");
        }

        var user = new User { Email = request.Email, PasswordHash = string.Empty };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Ok(new AuthResponse(tokenService.Generate(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        return Ok(new AuthResponse(tokenService.Generate(user)));
    }
}
