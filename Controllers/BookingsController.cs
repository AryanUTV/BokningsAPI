using BokningsApi.Data;
using BokningsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BokningsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Booking>>> GetAll()
    {
        return await _db.Bookings.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetById(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        return booking;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Booking>> Create(Booking booking)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        booking.BookedBy = username;

        if (booking.EndTime <= booking.StartTime)
        {
            return BadRequest("Sluttiden måste vara efter starttiden.");
        }

        var roomExists = await _db.Rooms.AnyAsync(r => r.Id == booking.RoomId);
        if (!roomExists)
        {
            return NotFound("Lokalen finns inte.");
        }

        var overlaps = await _db.Bookings.AnyAsync(b =>
            b.RoomId == booking.RoomId &&
            b.StartTime < booking.EndTime &&
            b.EndTime > booking.StartTime);

        if (overlaps)
        {
            return Conflict("Lokalen är redan bokad under den tiden.");
        }

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return Created($"/api/bookings/{booking.Id}", booking);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.BookedBy != User.Identity?.Name)
        {
            return Forbid();
        }

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}