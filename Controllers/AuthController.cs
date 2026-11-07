using BokningsApi.Data;
using BokningsApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BokningsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Password.Length < 8)
        {
            return BadRequest("Användarnamn krävs och lösenordet måste vara minst 8 tecken.");
        }

        var exists = await _db.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
        {
            return Conflict("Användarnamnet är upptaget.");
        }

        var user = new User { Username = request.Username };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{user.Id}", new { user.Id, user.Username });
    }
}
