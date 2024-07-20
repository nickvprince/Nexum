using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.DbServices.Interfaces;

namespace API.Services.DbServices
{
    public class DbPermissionService : IDbPermissionService
    {
        private readonly AppDbContext _appDbContext;
        public DbPermissionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Permission?> CreateAsync(Permission? permission)
        {
            if (permission != null)
            {
                try
                {
                    await _appDbContext.Permissions.AddAsync(permission);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return await _appDbContext.Permissions
                            .Where(p => p.Id == permission.Id)
                            .FirstAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the permission: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<Permission?> UpdateAsync(Permission? permission)
        {
            if (permission != null)
            {
                try
                {
                    var existingPermission = await _appDbContext.Permissions
                        .Where(p => p.Id == permission.Id)
                        .FirstAsync();
                    if (existingPermission != null)
                    {
                        _appDbContext.Entry(existingPermission).CurrentValues.SetValues(permission);

                        var result = await _appDbContext.SaveChangesAsync();
                        if (result >= 0)
                        {
                            return await _appDbContext.Permissions
                                .Where(p => p.Id == permission.Id)
                                .FirstAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the permission: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var permission = await _appDbContext.Permissions
                    .Where(p => p.Id == id)
                    .FirstAsync();
                if (permission != null)
                {
                    _appDbContext.Permissions.Remove(permission);
                    var result = await _appDbContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the permission: {ex.Message}");
            }
            return false;
        }

        public async Task<Permission?> GetAsync(int id)
        {
            try
            {
                var permission = await _appDbContext.Permissions
                    .Where(p => p.Id == id)
                    .FirstAsync();
                if (permission != null)
                {
                    return permission;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the permission: {ex.Message}");
            }
            return null;
        }

        public async Task<ICollection<Permission>?> GetAllAsync()
        {
            try
            {
                var permissions = await _appDbContext.Permissions.ToListAsync();
                if (permissions != null)
                {
                    if (permissions.Any())
                    {
                        return permissions;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting all permissions: {ex.Message}");
            }
            return null;
        }
    }
}
