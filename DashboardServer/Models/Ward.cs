namespace DashboardServer.Models;

/// <summary>
/// 病棟マスタ
/// </summary>
public class Ward
{
    /// <summary>
    /// 病棟ID
    /// </summary>
    public string WardId { get; set; } = string.Empty;

    /// <summary>
    /// 病棟名
    /// </summary>
    public string WardName { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 表示するかどうか
    /// </summary>
    public bool IsDisplay { get; set; }

    /// <summary>
    /// グラフの色（16進数カラーコード）
    /// </summary>
    public string? Color { get; set; }
}
