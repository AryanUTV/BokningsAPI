using System.Security.Claims;
using BokningsApi.Controllers;
using BokningsApi.Data;
using BokningsApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BokningsApi.Tests;

public class BookingsControllerTests
{
    private static AppDbContext CreateDb()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        db.Rooms.Add(new Room { Name = "Grupprum 3", Capacity = 6 });
        db.SaveChanges();

        return db;
    }

    // Skapar en controller där en låtsasanvändare är inloggad.
    // Skickar du null som användarnamn är ingen inloggad.
    private static BookingsController CreateController(AppDbContext db, string? username = "anna")
    {
        var controller = new BookingsController(db);
        var httpContext = new DefaultHttpContext();

        if (username != null)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, username) },
                "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static Booking MakeBooking(string bookedBy, int startHour, int endHour)
    {
        return new Booking
        {
            RoomId = 1,
            BookedBy = bookedBy,
            StartTime = new DateTime(2026, 11, 10, startHour, 0, 0),
            EndTime = new DateTime(2026, 11, 10, endHour, 0, 0)
        };
    }

    [Fact]
    public async Task Create_ValidBooking_ReturnsCreated()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var result = await controller.Create(MakeBooking("", 10, 12));

        Assert.IsType<CreatedResult>(result.Result);
    }

    [Fact]
    public async Task Create_OverlappingBooking_ReturnsConflict()
    {
        var db = CreateDb();
        db.Bookings.Add(MakeBooking("anna", 10, 12));
        db.SaveChanges();
        var controller = CreateController(db);

        var result = await controller.Create(MakeBooking("", 11, 13));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_BookingStartsWhenAnotherEnds_ReturnsCreated()
    {
        var db = CreateDb();
        db.Bookings.Add(MakeBooking("anna", 10, 12));
        db.SaveChanges();
        var controller = CreateController(db);

        var result = await controller.Create(MakeBooking("", 12, 14));

        Assert.IsType<CreatedResult>(result.Result);
    }

    [Fact]
    public async Task Create_RoomDoesNotExist_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = CreateController(db);
        var booking = MakeBooking("", 10, 12);
        booking.RoomId = 99;

        var result = await controller.Create(booking);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_EndBeforeStart_ReturnsBadRequest()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var result = await controller.Create(MakeBooking("", 12, 10));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithoutLogin_ReturnsUnauthorized()
    {
        var db = CreateDb();
        var controller = CreateController(db, username: null);

        var result = await controller.Create(MakeBooking("", 10, 12));

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_UsesLoggedInUsernameAsBookedBy()
    {
        var db = CreateDb();
        var controller = CreateController(db, "anna");

        await controller.Create(MakeBooking("någon annan", 10, 12));

        Assert.Equal("anna", Assert.Single(db.Bookings).BookedBy);
    }

    [Fact]
    public async Task GetById_ExistingBooking_ReturnsBooking()
    {
        var db = CreateDb();
        var booking = MakeBooking("anna", 10, 12);
        db.Bookings.Add(booking);
        db.SaveChanges();
        var controller = CreateController(db);

        var result = await controller.GetById(booking.Id);

        Assert.NotNull(result.Value);
        Assert.Equal("anna", result.Value.BookedBy);
    }

    [Fact]
    public async Task GetById_UnknownBooking_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_OwnBooking_RemovesItAndReturnsNoContent()
    {
        var db = CreateDb();
        var booking = MakeBooking("anna", 10, 12);
        db.Bookings.Add(booking);
        db.SaveChanges();
        var controller = CreateController(db, "anna");

        var result = await controller.Delete(booking.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Bookings);
    }

    [Fact]
    public async Task Delete_UnknownBooking_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_OtherUsersBooking_ReturnsForbid()
    {
        var db = CreateDb();
        var booking = MakeBooking("anna", 10, 12);
        db.Bookings.Add(booking);
        db.SaveChanges();
        var controller = CreateController(db, "bo");

        var result = await controller.Delete(booking.Id);

        Assert.IsType<ForbidResult>(result);
        Assert.Single(db.Bookings);
    }
}