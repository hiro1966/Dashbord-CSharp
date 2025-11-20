namespace DashboardServer.Models;

public class OutpatientChartData
{
    public List<string> Labels { get; set; } = new();
    public List<DatasetInfo> Datasets { get; set; } = new();
    public string Title { get; set; } = string.Empty;
}

public class DatasetInfo
{
    public string Label { get; set; } = string.Empty;
    public List<int> Data { get; set; } = new();
    public string BorderColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public bool Fill { get; set; } = false;
}
