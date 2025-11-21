namespace DashboardServer.Models;

/// <summary>
/// 診療科マスタ
/// </summary>
public class Department
{
    /// <summary>
    /// 診療科ID
    /// </summary>
    public string DepartmentId { get; set; } = string.Empty;

    /// <summary>
    /// 診療科名
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

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
