using API.DataAccess;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Services
{
    public class DbUserService : IDbUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbUserService(AppDbContext appDbContext, UserManager<ApplicationUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        public async Task<bool> CreateAsync(ApplicationUser? user, string? password)
        {
            if (user != null && !string.IsNullOrEmpty(password))
            {
                try
                {
                    IdentityResult result = await _userManager.CreateAsync(user, password);
                    return result.Succeeded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the user: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> UpdateAsync(ApplicationUser? user)
        {
            if (user != null)
            {
                try
                {
                    if (user.UserName != null)
                    {
                        ApplicationUser? existingUser = await _userManager.FindByNameAsync(user.UserName);
                        if (existingUser != null)
                        {
                            existingUser.FirstName = user.FirstName;
                            existingUser.LastName = user.LastName;
                            existingUser.Email = user.Email;
                            IdentityResult result = await _userManager.UpdateAsync(existingUser);
                            return result.Succeeded;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the user: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> UpdatePasswordAsync(string? username, string? currentPassword, string? newPassword)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                try
                {
                    ApplicationUser? user = await _userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        IdentityResult result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                        if (result.Succeeded)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while updating the user's password: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> DeleteAsync(string? id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    ApplicationUser? user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        IdentityResult result = await _userManager.DeleteAsync(user);
                        return result.Succeeded;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the user: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string? id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try 
                {
                    ApplicationUser? user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        return user;
                    }
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine($"An error occurred while getting the user: {ex.Message}"); 
                }
            }
            return null;
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string? username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    ApplicationUser? user = await _userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        return user;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while getting the user: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<ICollection<ApplicationUser>?> GetAllAsync()
        {
            try
            {
                var users = await _appDbContext.Users.ToListAsync();
                if (users != null)
                {
                    if (users.Any())
                    {
                        return users;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the users: {ex.Message}");
            }
            return null;
        }

        // currently unused
        public async Task<bool> CheckPasswordAsync(string? username, string? password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    ApplicationUser? user = await _userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        return await _userManager.CheckPasswordAsync(user, password);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while checking the user's password: {ex.Message}");
                }
            }
            return false;
        }
    }
}
