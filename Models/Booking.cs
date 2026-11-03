namespace BokningsApi.Models;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string BookedBy { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}