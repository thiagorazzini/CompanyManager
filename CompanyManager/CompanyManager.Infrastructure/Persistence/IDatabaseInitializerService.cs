namespace CompanyManager.Infrastructure.Persistence
{
    public interface IDatabaseInitializerService
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
