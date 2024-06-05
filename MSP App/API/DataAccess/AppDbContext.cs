using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedComponents.Entities;

namespace API.DataAccess
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public static async Task IntitalizeUserIdentities(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string username = "admin";
            string password = "Admin123!";

            string username2 = "user";
            string password2 = "User123!";

            // Seed custom roles
            var customRoles = new[] { "Admin", "User" };
            foreach (var role in customRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            User? adminUser = null;
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(username) == null)
            {
                adminUser = new User { UserName = username, FirstName = username };
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

            User? normalUser = null;
            if (await userManager.FindByNameAsync(username2) == null)
            {
                normalUser = new User { UserName = username2, FirstName = username2 };
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
                // You can now use adminUser.Id and normalUser.Id for seeding data
                SeedData(serviceProvider, adminUser.Id, normalUser.Id);
            }
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<PermissionSet> PermissionSets { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<UserPermissionSet> UserPermissionSets { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<Tenant>().ToTable("Tenants");
            modelBuilder.Entity<PermissionSet>().ToTable("PermissionSets");
            modelBuilder.Entity<UserTenant>().ToTable("UserTenants");
            modelBuilder.Entity<UserPermissionSet>().ToTable("UserPermissionSets");
            modelBuilder.Entity<ContactInfo>().ToTable("ContactInfos");
            modelBuilder.Entity<Device>().ToTable("Devices");

            // Configuring UserTenant entity as a many-to-many relationship
            modelBuilder.Entity<UserTenant>()
                .HasKey(ut => new { ut.UserId, ut.TenantId });

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.User)                         // Configuring the User relationship
                .WithMany(u => u.UserTenants)                  // A user can be associated with many UserTenant entries
                .HasForeignKey(ut => ut.UserId);               // Foreign key in UserTenant table

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.Tenant)                       // Configuring the Tenant relationship
                .WithMany(t => t.UserTenants)                  // A tenant can be associated with many UserTenant entries
                .HasForeignKey(ut => ut.TenantId);             // Foreign key in UserTenant table

            // Configuring UserPermissionSet entity as a many-to-many relationship
            modelBuilder.Entity<UserPermissionSet>()
                .HasKey(up => new { up.UserId, up.PermissionSetId });  // Composite key for the many-to-many relationship

            modelBuilder.Entity<UserPermissionSet>()
                .HasOne(up => up.User)                          // Configuring the User relationship
                .WithMany(u => u.UserPermissionSets)            // A user can be associated with many UserPermissionSet entries
                .HasForeignKey(up => up.UserId);                // Foreign key in UserPermissionSet table

            modelBuilder.Entity<UserPermissionSet>()
                .HasOne(up => up.PermissionSet)                 // Configuring the PermissionSet relationship
                .WithMany()                                     // No navigation property needed on PermissionSet side
                .HasForeignKey(up => up.PermissionSetId);       // Foreign key in UserPermissionSet table

            // Configuring PermissionSet entity relationships
            modelBuilder.Entity<PermissionSet>()
                .HasOne(ps => ps.Permission)                    // Configuring the Permission relationship
                .WithMany()                                     // No navigation property needed on Permission side
                .HasForeignKey(ps => ps.PermissionId);          // Foreign key in PermissionSet table

            // Configure Tenant and ContactInfo relationship
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.ContactInfo)
                .WithMany()
                .HasForeignKey(t => t.ContactInfoId);

            // Configure Tenant and Device relationship
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Tenant)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TenantId);

        }
        private static void SeedData(IServiceProvider serviceProvider, string adminUserId, string normalUserId)
        {
            using (var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                if (context.Permissions.Any())
                {
                    return; // DB has been seeded
                }

                // Add ContactInfos first
                context.ContactInfos.AddRange(
                    new ContactInfo { Name = "Tenant A", Email = "contact@tenantA.com", Phone = "123-456-7890", Address = "123 Main St", City = "Anytown", State = "Anystate", Zip = "12345", Country = "USA" },
                    new ContactInfo { Name = "Tenant B", Email = "contact@tenantB.com", Phone = "123-456-7891", Address = "456 Elm St", City = "Othertown", State = "Otherstate", Zip = "12346", Country = "USA" },
                    new ContactInfo { Name = "Tenant C", Email = "contact@tenantC.com", Phone = "123-456-7892", Address = "789 Oak St", City = "Sometown", State = "Somestate", Zip = "12347", Country = "USA" }
                );
                context.SaveChanges();

                // Retrieve inserted ContactInfos
                var contactInfo1 = context.ContactInfos.First(c => c.Email == "contact@tenantA.com");
                var contactInfo2 = context.ContactInfos.First(c => c.Email == "contact@tenantB.com");
                var contactInfo3 = context.ContactInfos.First(c => c.Email == "contact@tenantC.com");

                // Add Tenants
                context.Tenants.AddRange(
                    new Tenant { Name = "Tenant A", ContactInfoId = contactInfo1.Id, ApiKey = "7654-4522-6546-4231" },
                    new Tenant { Name = "Tenant B", ContactInfoId = contactInfo2.Id, ApiKey = "2313-5435-5432-7654" },
                    new Tenant { Name = "Tenant C", ContactInfoId = contactInfo3.Id, ApiKey = "4732-1849-3021-0438" }
                );
                context.SaveChanges();

                // Retrieve inserted Tenants
                var tenant1 = context.Tenants.First(t => t.Name == "Tenant A");
                var tenant2 = context.Tenants.First(t => t.Name == "Tenant B");
                var tenant3 = context.Tenants.First(t => t.Name == "Tenant C");

                // Add Permissions
                context.Permissions.AddRange(
                    new Permission { Name = "View", Description = "Can view tenant data" },
                    new Permission { Name = "Backup", Description = "Can manage backups for the tenant" },
                    new Permission { Name = "Device", Description = "Can manage devices for the tenant" }
                );
                context.SaveChanges();

                // Retrieve inserted Permissions
                var permission1 = context.Permissions.First(p => p.Name == "View");
                var permission2 = context.Permissions.First(p => p.Name == "Backup");
                var permission3 = context.Permissions.First(p => p.Name == "Device");

                // Add PermissionSets
                context.PermissionSets.AddRange(
                    new PermissionSet { PermissionId = permission1.Id, TenantId = tenant1.Id },
                    new PermissionSet { PermissionId = permission2.Id, TenantId = tenant2.Id },
                    new PermissionSet { PermissionId = permission3.Id, TenantId = tenant3.Id }
                );
                context.SaveChanges();

                // Retrieve inserted PermissionSets
                var permissionSet1 = context.PermissionSets.First(ps => ps.PermissionId == permission1.Id && ps.TenantId == tenant1.Id);
                var permissionSet2 = context.PermissionSets.First(ps => ps.PermissionId == permission2.Id && ps.TenantId == tenant2.Id);
                var permissionSet3 = context.PermissionSets.First(ps => ps.PermissionId == permission3.Id && ps.TenantId == tenant3.Id);

                // Add UserTenants
                context.UserTenants.AddRange(
                    new UserTenant { UserId = adminUserId, TenantId = tenant1.Id },
                    new UserTenant { UserId = normalUserId, TenantId = tenant2.Id }
                );

                // Add UserPermissionSets
                context.UserPermissionSets.AddRange(
                    new UserPermissionSet { UserId = adminUserId, PermissionSetId = permissionSet1.Id },
                    new UserPermissionSet { UserId = normalUserId, PermissionSetId = permissionSet2.Id }
                );

                // Add Devices
                context.Devices.AddRange(
                    new Device { Name = "Device 1", Type = "Type A", TenantId = tenant1.Id },
                    new Device { Name = "Device 2", Type = "Type B", TenantId = tenant1.Id },
                    new Device { Name = "Device 3", Type = "Type C", TenantId = tenant2.Id }
                );

                context.SaveChanges();
            }
        }
    }
}
