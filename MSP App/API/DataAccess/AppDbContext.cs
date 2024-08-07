using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Utilities;

namespace API.DataAccess
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public static async Task IntitalizeUserIdentities(IServiceProvider serviceProvider)
        {
            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string username = "admin";
            string password = "Admin123!";

            string username2 = "user";
            string password2 = "User123!";

            ApplicationUser? adminUser = null;
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(username) == null)
            {
                adminUser = new ApplicationUser { UserName = username, FirstName = username, Type = AccountType.MSP };
                var result = await userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create admin user");
                }
            }
            else
            {
                adminUser = await userManager.FindByNameAsync(username);
            }

            ApplicationUser? normalUser = null;
            if (await userManager.FindByNameAsync(username2) == null)
            {
                normalUser = new ApplicationUser { UserName = username2, FirstName = username2, Type = AccountType.MSP };
                var result = await userManager.CreateAsync(normalUser, password2);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create normal user");
                }
            }
            else
            {
                normalUser = await userManager.FindByNameAsync(username2);
            }

            // Seed data only if both users are created successfully
            if (adminUser != null && normalUser != null)
            {
                SeedData(serviceProvider, adminUser.Id, normalUser.Id);
            }
        }

        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationRolePermission> ApplicationRolePermissions { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantInfo> TenantInfos { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<MACAddress> MACAddresses { get; set; }
        public DbSet<DeviceAlert> DeviceAlerts { get; set; }
        public DbSet<DeviceLog> DeviceLogs { get; set; }
        public DbSet<DeviceJob> DeviceJobs { get; set; }
        public DbSet<DeviceJobInfo> DeviceJobInfos { get; set; }
        public DbSet<DeviceJobSchedule> DeviceJobSchedules { get; set; }
        public DbSet<DeviceBackup> DeviceBackups { get; set; }
        public DbSet<InstallationKey> InstallationKeys { get; set; }
        public DbSet<SoftwareFile> SoftwareFiles { get; set; }
        public DbSet<NASServer> NASServers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tenant and TenantInfo one-to-one relationship
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.TenantInfo)
                .WithOne(ti => ti.Tenant)
                .HasForeignKey<TenantInfo>(ti => ti.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant and NASServer one-to-many relationship
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.NASServers)
                .WithOne(d => d.Tenant)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the NASServer and Backup relationship
            modelBuilder.Entity<NASServer>()
                .HasMany(n => n.Backups)
                .WithOne(b => b.NASServer)
                .HasForeignKey(b => b.NASServerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant and Device one-to-many relationship
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Devices)
                .WithOne(d => d.Tenant)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Device and DeviceInfo one-to-one relationship
            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceInfo)
                .WithOne(di => di.Device)
                .HasForeignKey<DeviceInfo>(di => di.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the Device and Alert relationship
            modelBuilder.Entity<Device>()
                .HasMany(d => d.Alerts)
                .WithOne(a => a.Device)
                .HasForeignKey(a => a.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the AlertSeverity enum to be stored as a string
            modelBuilder.Entity<DeviceAlert>()
                .Property(a => a.Severity)
                .HasConversion<string>();

            // Configure the Device and Log relationship
            modelBuilder.Entity<Device>()
                .HasMany(d => d.Logs)
                .WithOne(a => a.Device)
                .HasForeignKey(a => a.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the AlertSeverity enum to be stored as a string
            modelBuilder.Entity<DeviceLog>()
                .Property(l => l.Type)
                .HasConversion<string>();

            // DeviceInfo and MacAddress one-to-many relationship
            modelBuilder.Entity<DeviceInfo>()
                .HasMany(di => di.MACAddresses)
                .WithOne(ma => ma.DeviceInfo)
                .HasForeignKey(ma => ma.DeviceInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the DeviceType enum to be stored as a string
            modelBuilder.Entity<DeviceInfo>()
                .Property(di => di.Type)
                .HasConversion<string>();

            // Configure the DeviceStatus enum to be stored as a string
            modelBuilder.Entity<Device>()
                .Property(d => d.Status)
                .HasConversion<string>();

            /*// Configure the Device and Backup relationship
            modelBuilder.Entity<Device>()
                .HasMany(d => d.Backups)
                .WithOne(j => j.Device)
                .HasForeignKey(j => j.DeviceId)
                .OnDelete(DeleteBehavior.NoAction);*/

            // Configure the Device and Job relationship
            modelBuilder.Entity<Device>()
                .HasMany(d => d.Jobs)
                .WithOne(j => j.Device)
                .HasForeignKey(j => j.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the DeviceJobStatus enum to be stored as a string
            modelBuilder.Entity<DeviceJob>()
                .Property(d => d.Status)
                .HasConversion<string>();

            // DeviceJob and DeviceJobInfo one-to-one relationship
            modelBuilder.Entity<DeviceJob>()
                .HasOne(dj => dj.Settings)
                .WithOne(dji => dji.DeviceJob)
                .HasForeignKey<DeviceJobInfo>(dji => dji.DeviceJobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the DeviceJobType enum to be stored as a string
            modelBuilder.Entity<DeviceJobInfo>()
                .Property(d => d.Type)
                .HasConversion<string>();

            /*// DeviceJobInfo and NASServer one-to-one relationship
            modelBuilder.Entity<DeviceJobInfo>()
                .HasOne(dji => dji.NASServer)
                .WithOne()
                .HasForeignKey<DeviceJobInfo>(dji => dji.NASServerId)
                .OnDelete(DeleteBehavior.NoAction);*/

            // Tenant and InstallationKey one-to-many relationship
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.InstallationKeys)
                .WithOne(ik => ik.Tenant)
                .HasForeignKey(ik => ik.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant and ApplicationRolePermission one-to-many relationship
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.RolePermissions)
                .WithOne(rp => rp.Tenant)
                .HasForeignKey(rp => rp.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-many relationship between ApplicationRole and Permission
            modelBuilder.Entity<ApplicationRolePermission>()
                .HasKey(rp => rp.Id);

            modelBuilder.Entity<ApplicationRolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId, rp.TenantId })
                .IsUnique();

            // Unique index for non-tenant permissions
            /*modelBuilder.Entity<ApplicationRolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasFilter("[TenantId] IS NULL");*/

            /*modelBuilder.Entity<ApplicationRolePermission>()
                .Property(rp => rp.TenantId)
                .IsRequired(false);*/

            modelBuilder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Configure many-to-many relationship between ApplicationUser and ApplicationRole
            modelBuilder.Entity<ApplicationUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Configure the SoftwareFileType enum to be stored as a string
            modelBuilder.Entity<SoftwareFile>()
                .Property(s => s.FileType)
                .HasConversion<string>();

            // Configure the InstallationKeyType enum to be stored as a string
            modelBuilder.Entity<InstallationKey>()
                .Property(ik => ik.Type)
                .HasConversion<string>();

            // Configure the AccountType enum to be stored as a string
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Type)
                .HasConversion<string>();

            // Configure the AccountType enum to be stored as a string
            modelBuilder.Entity<Permission>()
                .Property(p => p.Type)
                .HasConversion<string>();
        }
        private static void SeedData(IServiceProvider serviceProvider, string adminUserId, string normalUserId)
        {
            using (var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                if (context.Permissions.Any())
                {
                    return; // DB has been seeded
                }
                // Add Tenants first to get the generated IDs
                var tenant1 = new Tenant { Name = "TD Bank", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.tdbank.com", ApiBasePort = 8080, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant2 = new Tenant { Name = "RBC", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.rbc.com", ApiBasePort = 9090, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant3 = new Tenant { Name = "Scotiabank", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.scotiabank.com", ApiBasePort = 7070, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant4 = new Tenant { Name = "BMO", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.bmo.com", ApiBasePort = 6060, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant5 = new Tenant { Name = "CIBC", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.cibc.com", ApiBasePort = 5050, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant6 = new Tenant { Name = "HSBC", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.hsbc.com", ApiBasePort = 4040, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant7 = new Tenant { Name = "National Bank", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.nationalbank.com", ApiBasePort = 3030, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant8 = new Tenant { Name = "ING Direct", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.ingdirect.com", ApiBasePort = 2020, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant9 = new Tenant { Name = "Capital One", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.capitalone.com", ApiBasePort = 1010, ApiKeyServer = Guid.NewGuid().ToString() };
                var tenant10 = new Tenant { Name = "American Express", IsActive = true, ApiKey = Guid.NewGuid().ToString(), ApiBaseUrl = "https://api.americanexpress.com", ApiBasePort = 8888, ApiKeyServer = Guid.NewGuid().ToString() };


                context.Tenants.AddRange(tenant1, tenant2, tenant3, tenant4, tenant5, tenant6, tenant7, tenant8, tenant9, tenant10);
                context.SaveChanges();

                var tenants = context.Tenants.ToList();

                // Add TenantInfos with the correct TenantId
                var tenantInfo1 = new TenantInfo { Name = "Dave Seagel", Email = "dseagel@td.com", Phone = "123-456-7890", TenantId = tenant1.Id, Address = "123 Laurelwood dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "A1B 2C3" };
                var tenantInfo2 = new TenantInfo { Name = "John Doe", Email = "jdoe@rbc.com", Phone = "098-765-4321", TenantId = tenant2.Id, Address = "213 destiny dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "2C3 A1B" };
                var tenantInfo3 = new TenantInfo { Name = "Mel Sauve", Email = "msauve@scotia.com", Phone = "111-222-3333", TenantId = tenant3.Id, Address = "325 Conestoga dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "3C2 B1A" };
                var tenantInfo4 = new TenantInfo { Name = "Alice Smith", Email = "asmith@bmo.com", Phone = "222-333-4444", TenantId = tenant4.Id, Address = "456 Elm St", City = "Toronto", Country = "Canada", State = "Ontario", Zip = "B1A 2C3" };
                var tenantInfo5 = new TenantInfo { Name = "Bob Johnson", Email = "bjohnson@cibc.com", Phone = "333-444-5555", TenantId = tenant5.Id, Address = "789 Pine St", City = "Mississauga", Country = "Canada", State = "Ontario", Zip = "C2B 3A1" };
                var tenantInfo6 = new TenantInfo { Name = "Carol White", Email = "cwhite@hsbc.com", Phone = "444-555-6666", TenantId = tenant6.Id, Address = "321 Oak St", City = "Ottawa", Country = "Canada", State = "Ontario", Zip = "D3C 4B2" };
                var tenantInfo7 = new TenantInfo { Name = "Dan Brown", Email = "dbrown@nationalbank.com", Phone = "555-666-7777", TenantId = tenant7.Id, Address = "654 Maple St", City = "Vancouver", Country = "Canada", State = "British Columbia", Zip = "E4D 5C6" };
                var tenantInfo8 = new TenantInfo { Name = "Eve Black", Email = "eblack@ingdirect.com", Phone = "666-777-8888", TenantId = tenant8.Id, Address = "987 Birch St", City = "Calgary", Country = "Canada", State = "Alberta", Zip = "F5E 6D7" };
                var tenantInfo9 = new TenantInfo { Name = "Frank Green", Email = "fgreen@capitalone.com", Phone = "777-888-9999", TenantId = tenant9.Id, Address = "654 Cedar St", City = "Edmonton", Country = "Canada", State = "Alberta", Zip = "G6F 7E8" };
                var tenantInfo10 = new TenantInfo { Name = "Grace Blue", Email = "gblue@americanexpress.com", Phone = "888-999-0000", TenantId = tenant10.Id, Address = "321 Spruce St", City = "Montreal", Country = "Canada", State = "Quebec", Zip = "H7G 8F9" };

                context.TenantInfos.AddRange(tenantInfo1, tenantInfo2, tenantInfo3, tenantInfo4, tenantInfo5, tenantInfo6, tenantInfo7, tenantInfo8, tenantInfo9, tenantInfo10);
                context.SaveChanges();

                // Add NASServers
                var nas1 = new NASServer { Name = "NAS-TD Bank-001", BackupServerId = 0, Path = @"\\198.51.100.1\TD\SMBShare001", TenantId = tenant1.Id };
                var nas2 = new NASServer { Name = "NAS-RBC-001", BackupServerId = 0, Path = @"\\203.0.113.2\RBC\SMBShare001", TenantId = tenant2.Id };
                var nas3 = new NASServer { Name = "NAS-Scotiabank-001", BackupServerId = 0, Path = @"\\192.0.2.3\Scotiabank\SMBShare001", TenantId = tenant3.Id };
                var nas4 = new NASServer { Name = "NAS-BMO-001", BackupServerId = 0, Path = @"\\203.0.113.4\BMO\SMBShare001", TenantId = tenant4.Id };
                var nas5 = new NASServer { Name = "NAS-CIBC-001", BackupServerId = 0, Path = @"\\198.51.100.5\CIBC\SMBShare001", TenantId = tenant5.Id };
                var nas6 = new NASServer { Name = "NAS-HSBC-001", BackupServerId = 0, Path = @"\\192.0.2.6\HSBC\SMBShare001", TenantId = tenant6.Id };
                var nas7 = new NASServer { Name = "NAS-National Bank-001", BackupServerId = 0, Path = @"\\198.51.100.7\NationalBank\SMBShare001", TenantId = tenant7.Id };
                var nas8 = new NASServer { Name = "NAS-ING Direct-001", BackupServerId = 0, Path = @"\\203.0.113.8\INGDirect\SMBShare001", TenantId = tenant8.Id };
                var nas9 = new NASServer { Name = "NAS-Capital One-001", BackupServerId = 0, Path = @"\\192.0.2.9\CapitalOne\SMBShare001", TenantId = tenant9.Id };
                var nas10 = new NASServer { Name = "NAS-American Express-001", BackupServerId = 0, Path = @"\\198.51.100.10\AmericanExpress\SMBShare001", TenantId = tenant10.Id };

                context.NASServers.AddRange(nas1, nas2, nas3, nas4, nas5, nas6, nas7, nas8, nas9, nas10);
                context.SaveChanges();

                // Add Devices and DeviceInfos
                var device1 = new Device { TenantId = tenant1.Id, IsVerified = true, Status = DeviceStatus.Online };
                var device2 = new Device { TenantId = tenant2.Id, IsVerified = true, Status = DeviceStatus.Offline };
                var device3 = new Device { TenantId = tenant3.Id, IsVerified = true, Status = DeviceStatus.ServiceOffline };
                var device4 = new Device { TenantId = tenant4.Id, IsVerified = true, Status = DeviceStatus.BackupInProgress };
                var device5 = new Device { TenantId = tenant5.Id, IsVerified = true, Status = DeviceStatus.RestoreInProgress };
                var device6 = new Device { TenantId = tenant6.Id, IsVerified = true, Status = DeviceStatus.Online };
                var device7 = new Device { TenantId = tenant7.Id, IsVerified = true, Status = DeviceStatus.Offline };
                var device8 = new Device { TenantId = tenant8.Id, IsVerified = true, Status = DeviceStatus.ServiceOffline };
                var device9 = new Device { TenantId = tenant9.Id, IsVerified = true, Status = DeviceStatus.BackupInProgress };
                var device10 = new Device { TenantId = tenant10.Id, IsVerified = true, Status = DeviceStatus.RestoreInProgress };

                context.Devices.AddRange(device1, device2, device3,device4, device5, device6,device7, device8, device9, device10);
                context.SaveChanges();

                var deviceInfo1 = new DeviceInfo { Name = "TD-001", DeviceId = device1.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "203.0.113.11", Port = 8080, Type = DeviceType.Server };
                var deviceInfo2 = new DeviceInfo { Name = "RBC-001", DeviceId = device2.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "203.0.113.12", Port = 8081, Type = DeviceType.Server };
                var deviceInfo3 = new DeviceInfo { Name = "Scotia-001", DeviceId = device3.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "198.51.100.13", Port = 8082, Type = DeviceType.Server };
                var deviceInfo4 = new DeviceInfo { Name = "BMO-001", DeviceId = device4.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "198.51.100.14", Port = 8083, Type = DeviceType.Server };
                var deviceInfo5 = new DeviceInfo { Name = "CIBC-001", DeviceId = device5.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "192.0.2.15", Port = 8084, Type = DeviceType.Server };
                var deviceInfo6 = new DeviceInfo { Name = "HSBC-001", DeviceId = device6.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "192.0.2.16", Port = 8085, Type = DeviceType.Server };
                var deviceInfo7 = new DeviceInfo { Name = "NationalBank-001", DeviceId = device7.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "203.0.113.17", Port = 8086, Type = DeviceType.Server };
                var deviceInfo8 = new DeviceInfo { Name = "INGDirect-001", DeviceId = device8.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "203.0.113.18", Port = 8087, Type = DeviceType.Server };
                var deviceInfo9 = new DeviceInfo { Name = "CapitalOne-001", DeviceId = device9.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "198.51.100.19", Port = 8088, Type = DeviceType.Server };
                var deviceInfo10 = new DeviceInfo { Name = "AmericanExpress-001", DeviceId = device10.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "198.51.100.20", Port = 8089, Type = DeviceType.Server };

                context.DeviceInfos.AddRange(deviceInfo1, deviceInfo2, deviceInfo3, deviceInfo4, deviceInfo5, deviceInfo6, deviceInfo7, deviceInfo8, deviceInfo9, deviceInfo10);
                context.SaveChanges();

                var macAddress1 = new MACAddress { Address = "00:0a:95:9d:68:16", DeviceInfoId = deviceInfo1.Id };
                var macAddress2 = new MACAddress { Address = "00:0a:95:9d:68:17", DeviceInfoId = deviceInfo1.Id };
                var macAddress3 = new MACAddress { Address = "00:0a:95:9d:68:18", DeviceInfoId = deviceInfo2.Id };
                var macAddress4 = new MACAddress { Address = "00:0a:95:9d:68:19", DeviceInfoId = deviceInfo3.Id };
                var macAddress5 = new MACAddress { Address = "00:0a:95:9d:68:20", DeviceInfoId = deviceInfo3.Id };
                var macAddress6 = new MACAddress { Address = "00:0a:95:9d:68:21", DeviceInfoId = deviceInfo3.Id };
                var macAddress7 = new MACAddress { Address = "00:0a:95:9d:68:22", DeviceInfoId = deviceInfo4.Id };
                var macAddress8 = new MACAddress { Address = "00:0a:95:9d:68:23", DeviceInfoId = deviceInfo4.Id };
                var macAddress9 = new MACAddress { Address = "00:0a:95:9d:68:24", DeviceInfoId = deviceInfo5.Id };
                var macAddress10 = new MACAddress { Address = "00:0a:95:9d:68:25", DeviceInfoId = deviceInfo6.Id };
                var macAddress11 = new MACAddress { Address = "00:0a:95:9d:68:26", DeviceInfoId = deviceInfo6.Id };
                var macAddress12 = new MACAddress { Address = "00:0a:95:9d:68:27", DeviceInfoId = deviceInfo7.Id };
                var macAddress13 = new MACAddress { Address = "00:0a:95:9d:68:28", DeviceInfoId = deviceInfo7.Id };
                var macAddress14 = new MACAddress { Address = "00:0a:95:9d:68:29", DeviceInfoId = deviceInfo7.Id };
                var macAddress15 = new MACAddress { Address = "00:0a:95:9d:68:30", DeviceInfoId = deviceInfo8.Id };
                var macAddress16 = new MACAddress { Address = "00:0a:95:9d:68:31", DeviceInfoId = deviceInfo8.Id };
                var macAddress17 = new MACAddress { Address = "00:0a:95:9d:68:32", DeviceInfoId = deviceInfo9.Id };
                var macAddress18 = new MACAddress { Address = "00:0a:95:9d:68:33", DeviceInfoId = deviceInfo9.Id };
                var macAddress19 = new MACAddress { Address = "00:0a:95:9d:68:34", DeviceInfoId = deviceInfo10.Id };

                context.MACAddresses.AddRange(macAddress1, macAddress2, macAddress3, macAddress4, macAddress5, macAddress6, macAddress7, macAddress8, macAddress9, macAddress10, macAddress11, macAddress12, macAddress13, macAddress14, macAddress15, macAddress16, macAddress17, macAddress18, macAddress19);
                context.SaveChanges();

                // Add InstallationKeys
                var installationKey1 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant1.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey2 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant2.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey3 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant3.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey4 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant4.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey5 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant5.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey6 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant6.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey7 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant7.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey8 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant8.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey9 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant9.Id, Type = InstallationKeyType.Server, IsActive = false };
                var installationKey10 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant10.Id, Type = InstallationKeyType.Server, IsActive = false };

                context.InstallationKeys.AddRange(installationKey1, installationKey2, installationKey3, installationKey4, installationKey5, installationKey6, installationKey7, installationKey8, installationKey9, installationKey10);
                context.SaveChanges();


                // Add Permissions
                var permissions = new List<Permission>();
                var routePermissionList = ControllerUtilities.GetAllRoutes();
                routePermissionList = routePermissionList.DistinctBy(r => r.Permission).ToList();

                foreach (var (httpMethod, route, permissionName, type) in routePermissionList.Where(rpl => rpl.Type == PermissionType.Tenant))
                {
                    var permission = new Permission { Name = $"{permissionName}", Description = $"{EnumUtilities.EnumToString(type)} permission for {httpMethod} {route}", Type = type };
                    permissions.Add(permission);
                    context.Permissions.Add(permission);
                }
                context.SaveChanges();

                foreach (var (httpMethod, route, permissionName, type) in routePermissionList.Where(rpl => rpl.Type == PermissionType.System))
                {
                    var permission = new Permission { Name = $"{permissionName}", Description = $"{EnumUtilities.EnumToString(type)} permission for {httpMethod} {route}", Type = type };
                    permissions.Add(permission);
                    context.Permissions.Add(permission);
                }

                context.SaveChanges();

                // Add ApplicationRole
                var role1 = new ApplicationRole { Name = $"System Admin", Description = "Role for system admin", IsActive = true };
                context.ApplicationRoles.Add(role1);
                context.SaveChanges();

                foreach (var tenant in tenants)
                {
                    var roleTA = new ApplicationRole { Name = $"Tenant Admin - {tenant.Name}", Description = $"Admin Role for the tenant: {tenant.Name}", IsActive = true };
                    var roleTV = new ApplicationRole { Name = $"Tenant Viewer - {tenant.Name}", Description = $"Tenant role for viewing information for the tenant: {tenant.Name}", IsActive = true };
                    
                    context.ApplicationRoles.AddRange(roleTA, roleTV);
                }

                context.SaveChanges();
                var roles = context.ApplicationRoles.ToList();

                // Add UserRoles
                var userRole1 = new ApplicationUserRole { RoleId = role1.Id, UserId = adminUserId, IsActive = true };
                context.ApplicationUserRoles.Add(userRole1);
                context.SaveChanges();

                foreach (var role in roles.Where(r => r.Name.Contains("Tenant Admin")).ToList())
                {
                    var userRole = new ApplicationUserRole { RoleId = role.Id, UserId = adminUserId, IsActive = true };
                    context.ApplicationUserRoles.Add(userRole);
                }
                context.SaveChanges();
                foreach (var role in roles.Where(r => r.Name.Contains("Tenant Viewer")).ToList())
                {
                    var userRole = new ApplicationUserRole { RoleId = role.Id, UserId = normalUserId, IsActive = true };
                    context.ApplicationUserRoles.Add(userRole);
                }

                context.SaveChanges();

                // Add RolePermissions
                
                foreach (var permission in permissions.Where(p => p.Type == PermissionType.System))
                {
                    var rolePermission = new ApplicationRolePermission { RoleId = role1.Id, PermissionId = permission.Id };
                    context.ApplicationRolePermissions.Add(rolePermission);
                }

                context.SaveChanges();
                
                foreach (var tenant in tenants)
                {
                    var currRoleTA = roles.FirstOrDefault(r => r.Name.Contains($"Tenant Admin - {tenant.Name}"));
                    foreach (var permission in permissions.Where(p => p.Type == PermissionType.Tenant))
                    {
                        var rolePermission = new ApplicationRolePermission { RoleId = currRoleTA.Id, PermissionId = permission.Id, TenantId = tenant.Id };
                        context.ApplicationRolePermissions.Add(rolePermission);
                    }
                    var currRoleTV = roles.FirstOrDefault(r => r.Name.Contains($"Tenant Viewer - {tenant.Name}"));
                    foreach (var permission in permissions.Where(p => p.Type == PermissionType.Tenant && p.Name.Contains("Get")))
                    {
                        var rolePermission = new ApplicationRolePermission { RoleId = currRoleTV.Id, PermissionId = permission.Id, TenantId = tenant.Id };
                        context.ApplicationRolePermissions.Add(rolePermission);
                    }
                }
                context.SaveChanges();

                // Add SoftwareFiles

                var softwareFile1 = new SoftwareFile { UploadedFileName = "Nexum.exe", Version = "1.0.0", Tag = "alpha", FileType = SoftwareFileType.Nexum };
                var softwareFile2 = new SoftwareFile { UploadedFileName = "NexumServer.exe", Version = "1.0.0", Tag = "alpha", FileType = SoftwareFileType.NexumServer };
                var softwareFile3 = new SoftwareFile { UploadedFileName = "NexumService.exe", Version = "1.0.0", Tag = "alpha", FileType = SoftwareFileType.NexumService };

                context.SoftwareFiles.AddRange(softwareFile1, softwareFile2, softwareFile3);
                context.SaveChanges();

                // Add DeviceAlerts

                // Get all devices and their related information
                var deviceInfos = context.DeviceInfos.Include(d => d.Device).ThenInclude(d => d.Tenant).ToList();

                var random = new Random();
                var alertMessages = new List<(string message, AlertSeverity severity)>
                {
                    ("Device is offline", AlertSeverity.Critical),
                    ("Device is Online", AlertSeverity.Information),
                    ("Heart beat missed", AlertSeverity.Low),
                    ("Disk space low", AlertSeverity.Medium),
                    ("CPU usage high", AlertSeverity.High),
                    ("Temperature threshold exceeded", AlertSeverity.Critical),
                    ("Memory usage high", AlertSeverity.High),
                    ("Network latency detected", AlertSeverity.Medium),
                    ("Power supply failure", AlertSeverity.Critical),
                    ("Unauthorized access attempt", AlertSeverity.High)
                };

                var alerts = new List<DeviceAlert>();

                foreach (var device in deviceInfos)
                {
                    int numAlerts = random.Next(5, 11); // between 5 and 10 alerts
                    for (int j = 0; j < numAlerts; j++)
                    {
                        var alertMessage = alertMessages[random.Next(alertMessages.Count)];
                        var alert = new DeviceAlert
                        {
                            DeviceId = device.DeviceId,
                            Severity = alertMessage.severity,
                            Message = alertMessage.message,
                            Time = DateTime.Now.AddHours(random.Next(-48, 48)),
                            Acknowledged = random.Next(0, 2) == 1
                        };
                        alerts.Add(alert);
                    }
                }

                context.DeviceAlerts.AddRange(alerts);
                context.SaveChanges();

                // Add DeviceLogs

                var logMessages = new List<(string filename, string function, string message, int code, LogType type)>
                {
                    ("network.py", "connect()", "Network connection established", 100, LogType.Information),
                    ("network.py", "disconnect()", "Network connection lost", 101, LogType.Warning),
                    ("sensor.py", "readTemp()", "Temperature reading successful", 200, LogType.Information),
                    ("sensor.py", "readTemp()", "Temperature sensor error", 201, LogType.Error),
                    ("disk.py", "checkSpace()", "Disk space sufficient", 300, LogType.Information),
                    ("disk.py", "checkSpace()", "Disk space low", 301, LogType.Warning),
                    ("cpu.py", "monitorUsage()", "CPU usage normal", 400, LogType.Information),
                    ("cpu.py", "monitorUsage()", "High CPU usage detected", 401, LogType.Critical),
                    ("memory.py", "checkMemory()", "Memory usage normal", 500, LogType.Information),
                    ("memory.py", "checkMemory()", "Memory usage high", 501, LogType.Warning)
                };

                var logs = new List<DeviceLog>();

                foreach (var device in deviceInfos)
                {
                    int numLogs = random.Next(5, 11); // between 5 and 10 logs
                    for (int j = 0; j < numLogs; j++)
                    {
                        var logMessage = logMessages[random.Next(logMessages.Count)];
                        var log = new DeviceLog
                        {
                            DeviceId = device.DeviceId,
                            Type = logMessage.type,
                            Filename = logMessage.filename,
                            Function = logMessage.function,
                            Message = logMessage.message,
                            Code = logMessage.code,
                            Time = DateTime.Now.AddHours(random.Next(-24, 25))
                        };
                        logs.Add(log);
                    }
                }

                context.DeviceLogs.AddRange(logs);
                context.SaveChanges();

                // Add DeviceJobs

                var jobStatuses = Enum.GetValues(typeof(DeviceJobStatus)).Cast<DeviceJobStatus>().ToList();
                var jobTypes = Enum.GetValues(typeof(DeviceJobType)).Cast<DeviceJobType>().ToList();

                // Create jobs for each device
                var jobs = new List<DeviceJob>();

                // Get all DeviceInfos
                var devices = context.DeviceInfos.ToList();

                foreach (var device in devices)
                {
                    var job = new DeviceJob
                    {
                        DeviceId = device.DeviceId,
                        Name = $"Job for {device.Name}",
                        Status = jobStatuses[random.Next(jobStatuses.Count)],
                    };
                    if(job.Status != DeviceJobStatus.Complete || job.Status != DeviceJobStatus.NotStarted)
                    {
                        job.Progress = random.Next(0, 101);
                    }
                    jobs.Add(job);
                }

                context.DeviceJobs.AddRange(jobs);
                context.SaveChanges();

                var createdJobs = context.DeviceJobs.ToList();

                // Create job info for each job with randomized data
                var jobInfos = new List<DeviceJobInfo>();

                foreach (var job in createdJobs)
                {
                    var jobInfo = new DeviceJobInfo
                    {
                        DeviceJobId = job.Id,
                        BackupServerId = 0,
                        Type = jobTypes[random.Next(jobTypes.Count)], // Random job type
                        Sampling = random.Next(0, 2) == 0, // Random boolean value for Sampling
                        StartTime = DateTime.Now.AddMinutes(random.Next(-1000, 1000)).ToString("HH:mm"), // Random start time within approximately +/- 16 hours from now
                        EndTime = DateTime.Now.AddMinutes(random.Next(500, 2000)).ToString("HH:mm"), // Random end time within approximately 8 to 33 hours from now
                        UpdateInterval = random.Next(10, 60), // Random update interval between 10 and 60 minutes
                        Retention = random.Next(7, 30) // Random retention between 7 and 30 days
                    };
                    jobInfos.Add(jobInfo);
                }

                context.DeviceJobInfos.AddRange(jobInfos);
                context.SaveChanges();

                var createdJobInfos = context.DeviceJobInfos.ToList();

                // Create job schedules for each job info with randomized days
                var jobSchedules = new List<DeviceJobSchedule>();

                foreach (var jobInfo in createdJobInfos)
                {
                    var schedule = new DeviceJobSchedule
                    {
                        DeviceJobInfoId = jobInfo.Id,
                        Sunday = random.Next(0, 2) == 1,
                        Monday = random.Next(0, 2) == 1,
                        Tuesday = random.Next(0, 2) == 1,
                        Wednesday = random.Next(0, 2) == 1,
                        Thursday = random.Next(0, 2) == 1,
                        Friday = random.Next(0, 2) == 1,
                        Saturday = random.Next(0, 2) == 1
                    };
                    jobSchedules.Add(schedule);
                }

                context.DeviceJobSchedules.AddRange(jobSchedules);
                context.SaveChanges();

                // Add DeviceBackups

                // Function to generate a tenant-specific backup filename
                string GenerateBackupFilename(string tenantName, int backupNumber) => $"{tenantName}_Backup_{backupNumber}.bak";

                // Function to generate a subset path for backups
                string GenerateSubsetPath(string nasPath, int backupNumber) => $"{nasPath.Substring(nasPath.IndexOf('\\'))}\\Backup{backupNumber}";

                var backups = new List<DeviceBackup>();

                foreach (var deviceInfo in deviceInfos)
                {
                    var tenant = deviceInfo.Device.Tenant;
                    var tenantName = tenant.Name.Replace(" ", "");
                    var nas = context.NASServers.FirstOrDefault(n => n.TenantId == tenant.Id);

                    int numBackups = random.Next(2, 11); // Between 2 and 10 backups

                    for (int j = 1; j <= numBackups; j++)
                    {
                        var backup = new DeviceBackup
                        {
                            Client_Id = deviceInfo.ClientId,
                            Uuid = deviceInfo.Uuid,
                            TenantId = tenant.Id,
                            Filename = GenerateBackupFilename(tenantName, j),
                            Date = DateTime.Now.AddMinutes(random.Next(-1000, 1000)), // Random date within approximately +/- 16 hours from now
                            Path = GenerateSubsetPath(nas.Path, j),
                            NASServerId = nas.Id
                        };
                        backups.Add(backup);
                    }
                }

                context.DeviceBackups.AddRange(backups);
                context.SaveChanges();
            }
        }
    }
}
