using CRHoyWebhooks.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CRHoyWebhooks.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SubscribedUser> SubscribedUsers => Set<SubscribedUser>();
        public DbSet<RewardLog> RewardLogs => Set<RewardLog>();
    }
}
