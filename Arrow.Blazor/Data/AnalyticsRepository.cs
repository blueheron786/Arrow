using Arrow.Blazor.Models;
using Dapper;

namespace Arrow.Blazor.Data;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnalyticsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task TrackPageViewAsync(string pagePath)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = """
            INSERT INTO page_views (id, page_path, viewed_at_utc)
            VALUES (@Id, @PagePath, @ViewedAtUtc)
            """;

        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            PagePath = pagePath,
            ViewedAtUtc = DateTimeOffset.UtcNow
        });
    }

    public async Task<List<DailyPageViewStats>> GetDailyPageViewsAsync(string pagePath, DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = """
            SELECT 
                DATE(viewed_at_utc) as Date,
                COUNT(*) as ViewCount
            FROM page_views
            WHERE page_path = @PagePath
                AND viewed_at_utc >= @StartDate
                AND viewed_at_utc < @EndDate
            GROUP BY DATE(viewed_at_utc)
            ORDER BY Date ASC
            """;

        var results = await connection.QueryAsync<DailyPageViewStats>(sql, new
        {
            PagePath = pagePath,
            StartDate = startDate,
            EndDate = endDate.AddDays(1) // Include the end date
        });

        return results.ToList();
    }
}
