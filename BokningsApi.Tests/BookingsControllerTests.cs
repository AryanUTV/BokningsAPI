using BokningsApi.Controllers;
using BokningsApi.Data;
using BokningsApi.Models;
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

    [Fact]
    public async Task Create_ValidBooking_ReturnsCreated()
    {
        // Arrange: förbered
        var db = CreateDb();
        var controller = new BookingsController(db);
        var booking = new Booking
        {
            RoomId = 1,
            BookedBy = "Anna",
            StartTime = new DateTime(2026, 11, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 10, 12, 0, 0)
        };

        // Act: gör det som ska testas
        var result = await controller.Create(booking);

        // Assert: kontrollera resultatet
        Assert.IsType<CreatedResult>(result.Result);


    }

    [Fact]
    public async Task Create_OverlappingBooking_ReturnsConflict()
    {
        var db = CreateDb();
        db.Bookings.Add(new Booking
        {
            RoomId = 1,
            BookedBy = "Anna",
            StartTime = new DateTime(2026, 11, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 10, 12, 0, 0)
        });
        db.SaveChanges();
        var controller = new BookingsController(db);

        var result = await controller.Create(new Booking
        {
            RoomId = 1,
            BookedBy = "Bo",
            StartTime = new DateTime(2026, 11, 10, 11, 0, 0),
            EndTime = new DateTime(2026, 11, 10, 13, 0, 0)
        });

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_BookingStartsWhenAnotherEnds_ReturnsCreated()
    {
        var db = CreateDb();
        db.Bookings.Add(new Booking
        {
            RoomId = 1,
            BookedBy = "Anna",
            StartTime = new DateTime(2026, 11, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 10, 12, 0, 0)
        });
        db.SaveChanges();
        var controller = new BookingsController(db);

        var result = await controller.Create(new Booking
        {
            RoomId = 1,
            BookedBy = "Cia",
            StartTime = new DateTime(2026, 11, 10, 12, 0, 0),
            EndTime = new DateTime(2026, 11, 10, 14, 0, 0)
        });

        Assert.IsType<CreatedResult>(result.Result);
    }

    [Fact]
    public async Task Create_RoomDoesNotExist_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = new BookingsController(db);

        var result = await controller.Create(new Booking
        {
            RoomId = 99,
            BookedBy = "Dan",
            StartTime = new DateTime(2026, 11, 11, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 11, 12, 0, 0)
        });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_EndBeforeStart_ReturnsBadRequest()
    {
        var db = CreateDb();
        var controller = new BookingsController(db);

        var result = await controller.Create(new Booking
        {
            RoomId = 1,
            BookedBy = "Eva",
            StartTime = new DateTime(2026, 11, 12, 12, 0, 0),
            EndTime = new DateTime(2026, 11, 12, 10, 0, 0)
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
