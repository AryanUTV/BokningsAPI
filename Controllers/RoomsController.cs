using BokningsApi.Data;
using BokningsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BokningsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoomsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Room>>> GetAll()
    {
        return await _db.Rooms.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Room>> Create(Room room)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return Created($"/api/rooms/{room.Id}", room);
    }
}
