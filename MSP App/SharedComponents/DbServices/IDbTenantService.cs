﻿using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbTenantService
    {
        public Task<Tenant?> CreateAsync(Tenant? tenant);
        public Task<Tenant?> UpdateAsync(Tenant? tenant);
        public Task<bool> DeleteAsync(int id);
        public Task<Tenant?> GetAsync(int id);
        public Task<Tenant?> GetRichAsync(int id);
        public Task<Tenant?> GetByApiKeyAsync(string? apikey);
        public Task<ICollection<Tenant>?> GetAllAsync();
    }
}
