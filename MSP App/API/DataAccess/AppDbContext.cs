using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities;
using System.Net.Mail;

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
                adminUser = new ApplicationUser { UserName = username, FirstName = username };
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
                normalUser = new ApplicationUser { UserName = username2, FirstName = username2 };
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
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantInfo> TenantInfos { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<MACAddress> MACAddresses { get; set; }
        public DbSet<InstallationKey> InstallationKeys { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<SoftwareFile> SoftwareFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<Tenant>().ToTable("Tenants");
            modelBuilder.Entity<TenantInfo>().ToTable("TenantInfos");
            modelBuilder.Entity<Device>().ToTable("Devices");
            modelBuilder.Entity<DeviceInfo>().ToTable("DeviceInfos");
            modelBuilder.Entity<MACAddress>().ToTable("MACAddresses");
            modelBuilder.Entity<InstallationKey>().ToTable("InstallationKeys");
            modelBuilder.Entity<ApplicationRolePermission>().ToTable("RolePermissions");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<SoftwareFile>().ToTable("SoftwareFiles");

            // Tenant and TenantInfo one-to-one relationship
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.TenantInfo)
                .WithOne(ti => ti.Tenant)
                .HasForeignKey<TenantInfo>(ti => ti.TenantId)
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
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

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
                var tenant1 = new Tenant { Name = "TD", IsActive = true, ApiKey = Guid.NewGuid().ToString() };
                var tenant2 = new Tenant { Name = "RBC", IsActive = true, ApiKey = Guid.NewGuid().ToString() };
                var tenant3 = new Tenant { Name = "Scotia", IsActive = true, ApiKey = Guid.NewGuid().ToString() };

                context.Tenants.AddRange(tenant1, tenant2, tenant3);
                context.SaveChanges();

                // Add TenantInfos with the correct TenantId
                var tenantInfo1 = new TenantInfo { Name = "Dave Seagel", Email = "dseagel@td.com", Phone = "123-456-7890", TenantId = tenant1.Id, Address = "123 Laurelwood dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "A1B 2C3" };
                var tenantInfo2 = new TenantInfo { Name = "John Doe", Email = "jdoe@rbc.com", Phone = "098-765-4321", TenantId = tenant2.Id, Address = "213 destiny dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "2C3 A1B" };
                var tenantInfo3 = new TenantInfo { Name = "Mel Sauve", Email = "msauve@scotia.com", Phone = "111-222-3333", TenantId = tenant3.Id, Address = "325 Conestoga dr", City = "Waterloo", Country = "Canada", State = "Ontario", Zip = "3C2 B1A" };

                context.TenantInfos.AddRange(tenantInfo1, tenantInfo2, tenantInfo3);
                context.SaveChanges();

                // Add Devices and DeviceInfos
                var device1 = new Device { TenantId = tenant1.Id, IsVerified = true };
                var device2 = new Device { TenantId = tenant2.Id, IsVerified = true };
                var device3 = new Device { TenantId = tenant3.Id, IsVerified = false };

                context.Devices.AddRange(device1, device2, device3);
                context.SaveChanges();

                var deviceInfo1 = new DeviceInfo { Name = "TD-001 ", DeviceId = device1.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "192.168.1.1", Port = 8080, Type = DeviceType.Server };
                var deviceInfo2 = new DeviceInfo { Name = "RBC-001", DeviceId = device2.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "192.168.1.2", Port = 8081, Type = DeviceType.Server };
                var deviceInfo3 = new DeviceInfo { Name = "Scotia-001", DeviceId = device3.Id, ClientId = 0, Uuid = Guid.NewGuid().ToString(), IpAddress = "192.168.1.3", Port = 8082, Type = DeviceType.Server };

                context.DeviceInfos.AddRange(deviceInfo1, deviceInfo2, deviceInfo3);
                context.SaveChanges();

                var macAddress1 = new MACAddress { Address = "00:0a:95:9d:68:16", DeviceInfoId = deviceInfo1.Id };
                var macAddress2 = new MACAddress { Address = "00:0a:95:9d:68:17", DeviceInfoId = deviceInfo1.Id };
                var macAddress3 = new MACAddress { Address = "00:0a:95:9d:68:18", DeviceInfoId = deviceInfo2.Id };
                var macAddress4 = new MACAddress { Address = "00:0a:95:9d:68:19", DeviceInfoId = deviceInfo3.Id };
                var macAddress5 = new MACAddress { Address = "00:0a:95:9d:68:20", DeviceInfoId = deviceInfo3.Id };

                context.MACAddresses.AddRange(macAddress1, macAddress2, macAddress3, macAddress4, macAddress5);
                context.SaveChanges();

                // Add InstallationKeys
                var installationKey1 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant1.Id, IsActive = true };
                var installationKey2 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant2.Id, IsActive = true };
                var installationKey3 = new InstallationKey { Key = Guid.NewGuid().ToString(), TenantId = tenant3.Id, IsActive = false };

                context.InstallationKeys.AddRange(installationKey1, installationKey2, installationKey3);
                context.SaveChanges();

                // Add Permissions
                var permission1 = new Permission { Name = "View Tenant", Description = "Can view tenant" };
                var permission2 = new Permission { Name = "Edit Tenant", Description = "Can edit tenant" };
                var permission3 = new Permission { Name = "Delete Tenant", Description = "Can delete tenant" };

                context.Permissions.AddRange(permission1, permission2, permission3);
                context.SaveChanges();

                // Add ApplicationRole
                var role1 = new ApplicationRole { Name = "AdminRole", Description = "Role for admins" };
                var role2 = new ApplicationRole { Name = "UserRole", Description = "Role for users" };

                context.ApplicationRoles.AddRange(role1, role2);
                context.SaveChanges();

                // Add UserRoles
                var userRole1 = new ApplicationUserRole { RoleId = role1.Id, UserId = adminUserId, IsActive = true };
                var userRole2 = new ApplicationUserRole { RoleId = role2.Id, UserId = adminUserId, IsActive = true };
                var userRole3 = new ApplicationUserRole { RoleId = role2.Id, UserId = normalUserId, IsActive = true };

                context.ApplicationUserRoles.AddRange(userRole1, userRole2, userRole3);
                context.SaveChanges();

                // Add RolePermissions
                var rolePermission1 = new ApplicationRolePermission { RoleId = role1.Id, PermissionId = permission1.Id, TenantId = tenant1.Id};
                var rolePermission2 = new ApplicationRolePermission { RoleId = role1.Id, PermissionId = permission2.Id, TenantId = tenant2.Id };
                var rolePermission3 = new ApplicationRolePermission { RoleId = role2.Id, PermissionId = permission3.Id, TenantId = tenant3.Id };

                context.RolePermissions.AddRange(rolePermission1, rolePermission2, rolePermission3);
                context.SaveChanges();

                // Add SoftwareFiles

                var softwareFile1 = new SoftwareFile { UploadedFileName = "Nexum.exe", Version = "1.0.0-alpha", FileType = SoftwareFileType.Nexum };
                var softwareFile2 = new SoftwareFile { UploadedFileName = "NexumServer.exe", Version = "1.0.0-alpha", FileType = SoftwareFileType.NexumServer };
                var softwareFile3 = new SoftwareFile { UploadedFileName = "NexumService.exe", Version = "1.0.0-alpha", FileType = SoftwareFileType.NexumService };

                context.SoftwareFiles.AddRange(softwareFile1, softwareFile2, softwareFile3);
                context.SaveChanges();

            }
        }
    }
}
