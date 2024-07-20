using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Services.DbServices.Interfaces
{
    public interface IDbRoleService
    {
        public Task<ApplicationRole?> CreateAsync(ApplicationRole? role);
        public Task<ApplicationRole?> UpdateAsync(ApplicationRole? role);
        public Task<bool> DeleteAsync(string? id);
        public Task<ApplicationRole?> GetAsync(string? id);
        public Task<ICollection<ApplicationRole>?> GetAllAsync();
        public Task<ICollection<ApplicationRole>?> GetAllByUserIdAsync(string? userId);
        public Task<ICollection<ApplicationUserRole>?> GetAllUserRolesByUserIdAsync(string? userId);
        public Task<ICollection<ApplicationRolePermission>?> GetAllRolePermissionByIdAsync(string? roleId);
        public Task<bool> AssignAsync(ApplicationUserRole? userRole);
        public Task<bool> UnassignAsync(ApplicationUserRole? userRole);
        public Task<bool> AssignPermissionAsync(ApplicationRolePermission? rolePermission);
        public Task<bool> UnassignPermissionAsync(ApplicationRolePermission? rolePermission);
    }
}
