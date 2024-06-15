﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities;

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
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string username = "admin";
            string password = "Admin123!";

            string username2 = "user";
            string password2 = "User123!";

            /*// Seed custom roles
            var customRoles = new[] { "AdminRole", "UserRole" };
            foreach (var role in customRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role});
                }
            }

            ApplicationUser? adminUser = null;
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(username) == null)
            {
                adminUser = new ApplicationUser { UserName = username, FirstName = username };
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, customRoles.ElementAt(0));
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
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, customRoles.ElementAt(1));
                }
            }
            else
            {
                normalUser = await userManager.FindByNameAsync(username2);
            }

            // Seed data only if both users are created successfully
            if (adminUser != null && normalUser != null)
            {
                // Seed role claims
                var adminRole = await roleManager.FindByNameAsync("AdminRole");
                var userRole = await roleManager.FindByNameAsync("UserRole");

                SeedData(serviceProvider, adminUser.Id, normalUser.Id, adminRole.Id, userRole.Id);

            }
             */

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
        public DbSet<InstallationKey> InstallationKeys { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

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
            modelBuilder.Entity<InstallationKey>().ToTable("InstallationKeys");
            modelBuilder.Entity<ApplicationRolePermission>().ToTable("RolePermissions");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("UserRoles");

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
                var tenant1 = new Tenant { IsActive = true, ApiKey = "ApiKey1" };
                var tenant2 = new Tenant { IsActive = true, ApiKey = "ApiKey2" };
                var tenant3 = new Tenant { IsActive = true, ApiKey = "ApiKey3" };

                context.Tenants.AddRange(tenant1, tenant2, tenant3);
                context.SaveChanges();

                // Add TenantInfos with the correct TenantId
                var tenantInfo1 = new TenantInfo { Name = "TenantInfo1", Email = "tenant1@example.com", Phone = "1234567890", TenantId = tenant1.Id };
                var tenantInfo2 = new TenantInfo { Name = "TenantInfo2", Email = "tenant2@example.com", Phone = "0987654321", TenantId = tenant2.Id };
                var tenantInfo3 = new TenantInfo { Name = "TenantInfo3", Email = "tenant3@example.com", Phone = "1112223333", TenantId = tenant3.Id };

                context.TenantInfos.AddRange(tenantInfo1, tenantInfo2, tenantInfo3);
                context.SaveChanges();

                // Link TenantInfo to Tenant
                tenant1.TenantInfoId = tenantInfo1.Id;
                tenant2.TenantInfoId = tenantInfo2.Id;
                tenant3.TenantInfoId = tenantInfo3.Id;
                context.SaveChanges();

                // Add Devices and DeviceInfos
                var device1 = new Device { TenantId = tenant1.Id };
                var device2 = new Device { TenantId = tenant2.Id };
                var device3 = new Device { TenantId = tenant3.Id };

                context.Devices.AddRange(device1, device2, device3);
                context.SaveChanges();

                var deviceInfo1 = new DeviceInfo { Name = "Device1", DeviceId = device1.Id, ClientId = 1, Uuid = "uuid1", IpAddress = "192.168.1.1", Port = 8080, Type = "Desktop", MacAddresses = new List<string> { "00:0a:95:9d:68:16", "00:0a:95:9d:68:17" } };
                var deviceInfo2 = new DeviceInfo { Name = "Device2", DeviceId = device2.Id, ClientId = 2, Uuid = "uuid2", IpAddress = "192.168.1.2", Port = 8081, Type = "Laptop" , MacAddresses = new List<string> { "00:0a:95:9d:68:18" } };
                var deviceInfo3 = new DeviceInfo { Name = "Device3", DeviceId = device3.Id, ClientId = 3, Uuid = "uuid3", IpAddress = "192.168.1.3", Port = 8082, Type = "Desktop" , MacAddresses = new List<string> { "00:0a:95:9d:68:19", "00:0a:95:9d:68:20" } };

                context.DeviceInfos.AddRange(deviceInfo1, deviceInfo2, deviceInfo3);
                context.SaveChanges();

                // Link DeviceInfo to Device
                device1.DeviceInfoId = deviceInfo1.Id;
                device2.DeviceInfoId = deviceInfo2.Id;
                device3.DeviceInfoId = deviceInfo3.Id;
                context.SaveChanges();

                // Add InstallationKeys
                var installationKey1 = new InstallationKey { Key = "Key1", TenantId = tenant1.Id };
                var installationKey2 = new InstallationKey { Key = "Key2", TenantId = tenant2.Id };
                var installationKey3 = new InstallationKey { Key = "Key3", TenantId = tenant3.Id };

                context.InstallationKeys.AddRange(installationKey1, installationKey2, installationKey3);
                context.SaveChanges();

                // Add Permissions
                var permission1 = new Permission { Name = "View", Description = "Description1" };
                var permission2 = new Permission { Name = "Edit", Description = "Description2" };
                var permission3 = new Permission { Name = "Delete", Description = "Description3" };

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

            }
        }
    }
}
