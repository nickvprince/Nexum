using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbRoleService : IDbRoleService
    {
        private readonly AppDbContext _appDbContext;
        public DbRoleService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ApplicationRole?> CreateAsync(ApplicationRole? role)
        {
            if (role != null)
            {
                try
                {
                    await _appDbContext.ApplicationRoles.AddAsync(role);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.ApplicationRoles
                            .Where(r => r.Id == role.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the role: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<ApplicationRole?> UpdateAsync(ApplicationRole? role)
        {
            if (role != null)
            {
                try
                {
                    var existingRole = await _appDbContext.ApplicationRoles
                        .Where(r => r.Id == role.Id)
                        .FirstAsync();
                    if (existingRole != null)
                    {
                        _appDbContext.Entry(existingRole).CurrentValues.SetValues(role);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.ApplicationRoles
                                .Where(r => r.Id == role.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the role: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(string? id)
        {
            try
            {
                var role = await _appDbContext.ApplicationRoles
                    .Where(r => r.Id == id)
                    .FirstAsync();
                if (role != null)
                {
                    _appDbContext.ApplicationRoles.Remove(role);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the role: {ex.Message}");
            }
            return false;
        }

        public async Task<ApplicationRole?> GetAsync(string? id)
        {
            try
            {
                var role =  await _appDbContext.ApplicationRoles
                    .Where(r => r.Id == id)
                    .FirstAsync();
                if (role != null)
                {
                    return role;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the role: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<ApplicationRole>?> GetAllAsync()
        {
            try
            {
                var roles = await _appDbContext.ApplicationRoles
                    .Include(r => r!.RolePermissions!)
                        .ThenInclude(rp => rp.Permission)
                    .ToListAsync();
                if (roles != null)
                {
                    if (roles.Any())
                    {
                        return roles;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all roles: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<ApplicationRole>?> GetAllByUserIdAsync(string? userId)
        {
            try
            {
                var userRoles = await _appDbContext.ApplicationUserRoles
                    .Where(ur => ur.UserId == userId)
                    .Include(u => u.Role)
                        .ThenInclude(r => r!.RolePermissions!)
                            .ThenInclude(rp => rp.Permission)
                    .ToListAsync();
                if (userRoles != null)
                {
                    var roles = userRoles.Select(ur => ur.Role!)
                        .ToList();
                    if (roles != null)
                    {
                        if (roles.Any())
                        {
                            return roles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the roles for the user: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<ApplicationUserRole>?> GetAllUserRolesByUserIdAsync(string? userId)
        {
            try
            {
                var userRoles = await _appDbContext.ApplicationUserRoles
                    .Where(ur => ur.UserId == userId)
                    .Include(u => u.Role)
                        .ThenInclude(r => r!.RolePermissions!)
                            .ThenInclude(rp => rp.Permission)
                    .ToListAsync();
                if (userRoles != null)
                {
                    if (userRoles.Any())
                    {
                        return userRoles;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the roles for the user: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<ApplicationRolePermission>?> GetAllRolePermissionByIdAsync(string? roleId)
        {
            try
            {
                var rolePermissions = await _appDbContext.ApplicationRolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Include(r => r.Permission)
                    .ToListAsync();
                if (rolePermissions != null)
                {
                    if (rolePermissions.Any())
                    {
                        return rolePermissions;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the permissions for the role: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AssignAsync(ApplicationUserRole? userRole)
        {
            try
            {
                if (userRole != null)
                {
                    userRole.IsActive = false;
                    await _appDbContext.ApplicationUserRoles.AddAsync(userRole);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding the role to the user: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> UnassignAsync(ApplicationUserRole? userRole)
        {
            try
            {
                if (userRole != null)
                {
                    _appDbContext.ApplicationUserRoles.Remove(userRole);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while removing the role from the user: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> AssignPermissionAsync(ApplicationRolePermission? rolePermission)
        {
            try
            {
                if (rolePermission != null)
                {
                    await _appDbContext.ApplicationRolePermissions.AddAsync(rolePermission);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding the permission to the role: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> UnassignPermissionAsync(ApplicationRolePermission? rolePermission)
        {
            try
            {
                if (rolePermission != null)
                {
                    _appDbContext.ApplicationRolePermissions.Remove(rolePermission);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while removing the permission from the role: {ex.Message}");
            }
            return false;
        }
    }
}
