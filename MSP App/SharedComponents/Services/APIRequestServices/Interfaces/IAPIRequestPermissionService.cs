using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.APIRequestServices.Interfaces
{
    public interface IAPIRequestPermissionService
    {
        //Functions that can be added for debugging / future purposes (working code)
        /*public Task<Permission?> CreateAsync(PermissionCreateRequest request);
        public Task<Permission?> UpdateAsync(PermissionUpdateRequest request);
        public Task<bool> DeleteAsync(int id);*/
        public Task<Permission?> GetAsync(int id);
        public Task<ICollection<Permission>?> GetAllAsync();
    }
}
