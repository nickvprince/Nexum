
namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbSecurityService
    {
        public Task<bool> ValidateAPIKey(string? apikey);
    }
}
