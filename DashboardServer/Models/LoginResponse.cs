namespace DashboardServer.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
    public string? StaffLevel { get; set; }
}
