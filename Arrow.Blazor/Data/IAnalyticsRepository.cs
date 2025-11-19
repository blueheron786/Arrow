using Arrow.Blazor.Models;

namespace Arrow.Blazor.Data;

public interface IAnalyticsRepository
{
    Task TrackPageViewAsync(string pagePath);
    Task<List<DailyPageViewStats>> GetDailyPageViewsAsync(string pagePath, DateTime startDate, DateTime endDate);
}
