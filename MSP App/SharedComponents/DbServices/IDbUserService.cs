﻿using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbUserService
    {
        public Task<bool> AddAsync(ApplicationUser user);
        public Task<bool> EditAsync(ApplicationUser user);
        public Task<bool> DeleteAsync(string? id);
        public Task<ApplicationUser?> GetAsync(string? id);
        public Task<ICollection<ApplicationUser>> GetAllAsync();
    }
}
