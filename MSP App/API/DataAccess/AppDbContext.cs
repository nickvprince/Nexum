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

            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(username) == null)
            {
                User user = new User { UserName = username, FirstName = username };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, customRoles.ElementAt(0));
                }
            }
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(username2) == null)
            {
                User user = new User { UserName = username2, FirstName = username2 };
                var result = await userManager.CreateAsync(user, password2);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, customRoles.ElementAt(1));
                }
            }
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permission>().HasData(
                new Permission()
                {
                    Id = 1,
                    Name = "Create User",
                    Description = "Create a new user",
                    IsActive = true
                },
                new Permission()
                {
                    Id = 2,
                    Name = "Edit User",
                    Description = "Edit an existing user",
                    IsActive = true
                },
                new Permission()
                {
                    Id = 3,
                    Name = "Delete User",
                    Description = "Delete an existing user",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Group>().HasData(
                new Group()
                {
                    Id = 1,
                    Name = "Banks",
                    Description = "Group for managing banks",
                    IsActive = true
                },
                new Group()
                {
                    Id = 2,
                    Name = "Tech",
                    Description = "Group for managing tech businesses",
                    IsActive = true
                },
                new Group()
                {
                    Id = 3,
                    Name = "Culinary",
                    Description = "Group for managing culinary businesses",
                    IsActive = true
                }
            );
        }

    }
}
