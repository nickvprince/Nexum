﻿using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.RoleRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Services
{
    public interface IRoleService
    {
        public Task<ApplicationRole?> CreateAsync(RoleCreateRequest request);
        public Task<ApplicationRole?> UpdateAsync(RoleUpdateRequest request);
        public Task<bool> DeleteAsync(string? id);
        public Task<ApplicationRole?> GetAsync(string? id);
        public Task<ICollection<ApplicationRole>?> GetAllAsync();
        public Task<ICollection<ApplicationRole>?> GetAllByUserIdAsync(string? userId);
        public Task<ICollection<ApplicationUserRole>?> GetAllUserRolesByUserIdAsync(string? userId);
        public Task<ICollection<ApplicationRolePermission>?> GetAllRolePermissionByIdAsync(string? roleId);
        public Task<bool> AssignAsync(RoleAssignRequest request);
        public Task<bool> UnassignAsync(RoleUnassignRequest request);
        public Task<bool> AssignPermissionAsync(RoleAssignPermissionRequest request);
        public Task<bool> UnassignPermissionAsync(RoleUnassignPermissionRequest request);
    }
}
