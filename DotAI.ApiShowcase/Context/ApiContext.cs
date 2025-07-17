using DotAI.ApiShowcase.Entites;
using Microsoft.EntityFrameworkCore;

namespace DotAI.ApiShowcase.Context
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}