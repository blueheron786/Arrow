namespace Arrow.Blazor.Services;

public interface IPageViewTracker
{
    Task TrackPageViewAsync(string pagePath);
}
