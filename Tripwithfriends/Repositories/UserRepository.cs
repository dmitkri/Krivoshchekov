using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Data;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;

namespace Tripwithfriends.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
}


