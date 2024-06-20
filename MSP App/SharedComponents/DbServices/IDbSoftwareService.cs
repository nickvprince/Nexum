using SharedComponents.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.DbServices
{
    public interface IDbSoftwareService
    {
        public Task<SoftwareFile?> CreateAsync(SoftwareFile? softwareFile);
        public Task<SoftwareFile?> GetAsync(int id);
        public Task<SoftwareFile?> GetLatestNexumAsync();
        public Task<SoftwareFile?> GetLatestNexumServerAsync();
        public Task<SoftwareFile?> GetLatestNexumServiceAsync();
        public Task<ICollection<SoftwareFile>> GetAllAsync();
    }
}
