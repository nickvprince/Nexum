using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbPermissionService
    {
        public Task<Permission?> CreateAsync(Permission? permission);
        public Task<Permission?> UpdateAsync(Permission? permission);
        public Task<bool> DeleteAsync(int id);
        public Task<Permission?> GetAsync(int id);
        public Task<ICollection<Permission>?> GetAllAsync();
    }
}
