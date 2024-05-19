using API.DataAccess;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbGroupService : IDbGroupService
    {
        private readonly AppDbContext _appDbContext;

        public DbGroupService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public Task<bool> CreateAsync(Group group)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(Group group)
        {
            throw new NotImplementedException();
        }

        public Task<Group> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Group>> GetAllAsync()
        {
            return await _appDbContext.Groups.ToListAsync();
        }
    }
}
