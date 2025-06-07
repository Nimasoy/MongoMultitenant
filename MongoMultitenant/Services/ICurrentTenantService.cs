namespace MongoMultitenant.Services
{
    public interface ICurrentTenantService
    {
        string? TenantId { get; set; }
        Task<bool> CheckTenantAsync(string TenantId);
    }
}
