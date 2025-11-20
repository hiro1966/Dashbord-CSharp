namespace DashboardServer.Models;

public class DashboardData
{
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public string Title { get; set; } = string.Empty;
}
