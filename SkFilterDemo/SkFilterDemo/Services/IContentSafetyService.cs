namespace SkFilterDemo.Services
{
    public interface IContentSafetyService
    {
        Task<bool> IsSafeAsync(string text, CancellationToken ct = default);
    }
}
