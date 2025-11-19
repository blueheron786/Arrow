namespace Arrow.Blazor.Models;

public class PageView
{
    public Guid Id { get; set; }
    public string PagePath { get; set; } = string.Empty;
    public DateTimeOffset ViewedAtUtc { get; set; }
}
