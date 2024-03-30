using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SharedComponents.Entities;

namespace App.DataAccess
{
    public class DbContext : IdentityDbContext<User>
    {

    }
}
